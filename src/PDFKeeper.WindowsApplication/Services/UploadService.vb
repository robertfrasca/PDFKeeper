﻿'******************************************************************************
'* PDFKeeper -- Open Source PDF Document Management
'* Copyright (C) 2009-2021 Robert F. Frasca
'*
'* This file is part of PDFKeeper.
'*
'* PDFKeeper is free software: you can redistribute it and/or modify
'* it under the terms of the GNU General Public License as published by
'* the Free Software Foundation, either version 3 of the License, or
'* (at your option) any later version.
'*
'* PDFKeeper is distributed in the hope that it will be useful,
'* but WITHOUT ANY WARRANTY; without even the implied warranty of
'* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'* GNU General Public License for more details.
'*
'* You should have received a copy of the GNU General Public License
'* along with PDFKeeper.  If not, see <http://www.gnu.org/licenses/>.
'******************************************************************************
Imports iText.Kernel.Crypto

Public NotInheritable Class UploadService
    Private Shared s_Instance As UploadService
    Private m_CanUploadCycleStart As Boolean = True
    Private isUploadCycleExecuting As Boolean
    Private Shared s_FlaggedDocumentsUploaded As Boolean

    Private Sub New()
        ' Prevents multiple instances of this class.
    End Sub

    ''' <summary>
    ''' Allows single instance access to the class.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared ReadOnly Property Instance As UploadService
        Get
            If s_Instance Is Nothing Then
                s_Instance = New UploadService
            End If
            Return s_Instance
        End Get
    End Property

    ''' <summary>
    ''' Gets/Sets if the upload cycle can start.
    ''' </summary>
    ''' <value>True or False</value>
    ''' <returns>True or False</returns>
    ''' <remarks></remarks>
    Public Property CanUploadCycleStart As Boolean
        Get
            If m_CanUploadCycleStart = False Or isUploadCycleExecuting Then
                Return False
            Else
                Return True
            End If
        End Get
        Set(value As Boolean)
            WaitUntilUploadCycleIsNotExecuting()
            m_CanUploadCycleStart = value
        End Set
    End Property

#Disable Warning CA1822 ' Mark members as static
    ''' <summary>
    ''' If flagged documents were uploaded during the last upload cycle.
    ''' </summary>
    ''' <returns>True or False</returns>
    Public ReadOnly Property FlaggedDocumentsUploaded As Boolean
#Enable Warning CA1822 ' Mark members as static
        Get
            Return s_FlaggedDocumentsUploaded
        End Get
    End Property

    Public Sub ExecuteUploadCycle()
        If CanUploadCycleStart = False Then
            Throw New InvalidOperationException(
                My.Resources.UploadCycleCannotBeStarted)
        End If
        isUploadCycleExecuting = True
        s_FlaggedDocumentsUploaded = False
        EnsureConfiguredUploadFoldersExist()    ' Step 1
        StagePdfsAndSupplementalDataForUpload() ' Step 2
        RemoveDeleteFilesFromUploadFolders()    ' Step 3
        UploadFoldersCleanup()                  ' Step 4
        UploadStagedPdfsAndSupplementalData()   ' Step 5
        isUploadCycleExecuting = False
    End Sub

    Public Sub WaitUntilUploadCycleIsNotExecuting()
        Do While isUploadCycleExecuting
            Threading.Thread.Sleep(1000)
        Loop
    End Sub

#Region "Step 1: EnsureConfiguredUploadFoldersExist"
    ''' <summary>
    ''' Ensures a folder for each Upload Configuration exists in the Upload
    ''' folder.
    ''' </summary>
    ''' <remarks></remarks>
    Private Shared Sub EnsureConfiguredUploadFoldersExist()
        For Each config As String In
            UploadFolderConfigurationUtil.GetAllConfigFileNames(True, False)
            Directory.CreateDirectory(IO.Path.Combine(UserProfile.UploadPath,
                                                      config))
        Next
    End Sub
#End Region

#Region "Step 2: StagePdfsAndSupplementalDataForUpload"
    ''' <summary>
    ''' Stages each PDF and its cooresponding supplemental data to the Upload
    ''' Staging folder.
    ''' </summary>
    ''' <remarks></remarks>
    Private Shared Sub StagePdfsAndSupplementalDataForUpload()
        Dim pdfs = Directory.GetFiles(UserProfile.UploadPath,
                                      "*.pdf",
                                      SearchOption.AllDirectories).OrderBy(
                                      Function(f) New FileInfo(f).LastWriteTime)
        For Each pdfPath In pdfs
            Dim fileInfo As New FileInfo(pdfPath)
            fileInfo.WaitWhileIsInUse()
            Dim pdfFile As New PdfFile(pdfPath)
            If pdfFile.ContainsOwnerPassword = False Then
                Dim uploadFolderName As String =
                    pdfPath.Substring(UserProfile.UploadPath.Length + 1)
                If uploadFolderName = IO.Path.GetFileName(pdfPath) Then
                    uploadFolderName = UserProfile.UploadPath
                Else
                    uploadFolderName =
                        uploadFolderName.Substring(0,
                                                   uploadFolderName.IndexOf(
                                                   IO.Path.DirectorySeparatorChar))
                End If
                Try
                    Dim outputPdfPath As String = fileInfo.GenerateUploadStagingFilePath
                    Dim pdfReader As New PdfMetadataReader(pdfPath)
                    If UploadFolderConfigurationUtil.IsFolderConfigured(uploadFolderName) Then
                        WriteNewPdfAndSupplementalData(pdfPath,
                                                       outputPdfPath,
                                                       uploadFolderName)
                    Else
                        If pdfReader.Title.Length > 0 And
                            pdfReader.Author.Length > 0 And
                            pdfReader.Subject.Length > 0 Then
                            StageExistingPdfAndSupplementalData(pdfPath,
                                                                outputPdfPath)
                        End If
                    End If
                Catch ex As BadPasswordException    ' Ignore the file.            
                End Try
            End If
        Next
    End Sub

    Private Shared Sub WriteNewPdfAndSupplementalData(ByVal inputPdfPath As String,
                                                      ByVal outputPdfPath As String,
                                                      ByVal uploadFolderConfigName As String)
        Dim pdfInfoPropHelper As New PdfMetadataHelper(inputPdfPath, Nothing)
        pdfInfoPropHelper.Write(outputPdfPath, uploadFolderConfigName)
        Dim uploadFolderConfigHelper As _
            New UploadFolderConfigurationHelper(uploadFolderConfigName)
        Dim uploadFolderConfig As UploadFolderConfiguration =
            uploadFolderConfigHelper.Read
        Dim suppDataHelper As New PdfSupplementalDataHelper(outputPdfPath)
        Dim suppData As New PdfSupplementalData
        Dim flag As String = uploadFolderConfig.FlagDocument.ToString(CultureInfo.CurrentCulture)
        Dim state As Integer = 0
        If flag Then
            state = 1
        End If
        With suppData
            .Notes = String.Empty
            .Category = uploadFolderConfig.CategoryPrefill
            .FlagState = state
        End With
        suppDataHelper.Write(suppData.Notes,
                             suppData.Category,
                             suppData.TaxYear,
                             suppData.FlagState)
        Dim inputPdfGuidPath As String =
            New FileInfo(inputPdfPath).AppendGuidToName(Nothing)
        IO.File.Copy(inputPdfPath, inputPdfGuidPath)
        Dim fileInfo As New FileInfo(inputPdfGuidPath)
        fileInfo.DeleteToRecycleBin()
        Try
            IO.File.Move(inputPdfPath,
                         IO.Path.ChangeExtension(inputPdfPath,
                                                 "delete"))
        Catch ex As IOException
        Catch ex As UnauthorizedAccessException
        End Try
    End Sub

    Private Shared Sub StageExistingPdfAndSupplementalData(ByVal sourcePdfPath As String,
                                                           ByVal targetPdfPath As String)
        Dim sourceXmlPath As String = IO.Path.ChangeExtension(sourcePdfPath, "xml")
        Dim targetXmlPath As String = IO.Path.ChangeExtension(targetPdfPath, "xml")
        IO.File.Move(sourcePdfPath, targetPdfPath)
        If IO.File.Exists(sourceXmlPath) Then
            IO.File.Move(sourceXmlPath, targetXmlPath)
        End If
    End Sub
#End Region

#Region "Step 3: RemoveDeleteFilesFromUploadFolders"
    ''' <summary>
    ''' Deletes all files with a "delete" extension that are not in use from
    ''' the Upload folder and its sub-folders.
    ''' </summary>
    Private Shared Sub RemoveDeleteFilesFromUploadFolders()
        For Each deleteFile In Directory.GetFiles(UserProfile.UploadPath,
                                                  "*.delete",
                                                  SearchOption.AllDirectories)
            Try
                IO.File.Delete(deleteFile)
            Catch ex As IOException
            Catch ex As UnauthorizedAccessException
            End Try
        Next
    End Sub
#End Region

#Region "Step 4: UploadFoldersCleanup"
    ''' <summary>
    ''' Deletes all empty non configured folders from the Upload folder and any
    ''' empty sub-folders under each configured upload folder.
    ''' </summary>
    ''' <remarks></remarks>
    Private Shared Sub UploadFoldersCleanup()
        Dim subFolders As String() =
            Directory.GetDirectories(UserProfile.UploadPath)
        For Each subFolder In subFolders
            Dim dirInfo As New DirectoryInfo(subFolder)
            If UploadFolderConfigurationUtil.IsFolderConfigured(
                dirInfo.Name) Then
                ' When the folder is a configured folder then only
                ' delete empty sub-folders under the configured folder.
                Dim subFoldersL2 As String() =
                    Directory.GetDirectories(subFolder)
                For Each subFolderL2 In subFoldersL2
                    If Directory.GetFiles(subFolderL2).Any = False Then
                        Directory.Delete(subFolderL2, True)
                    End If
                Next
            Else
                If Directory.GetFiles(subFolder).Any = False Then
                    Directory.Delete(subFolder, True)
                End If
            End If
        Next
    End Sub
#End Region

#Region "Step 5: UploadStagedPdfsAndSupplementalData"
    ''' <summary>
    ''' Uploads all PDF files and supplemental data in the UploadStaging
    ''' folder except password protected PDF files.
    ''' </summary>
    ''' <remarks></remarks>
    Private Shared Sub UploadStagedPdfsAndSupplementalData()
        Dim pdfs = Directory.GetFiles(UserProfile.UploadStagingPath,
                                      "*.pdf",
                                      SearchOption.TopDirectoryOnly).OrderBy(
                                      Function(f) New FileInfo(f).LastWriteTime)
        For Each pdfPath In pdfs
            Dim fileInfo As New FileInfo(pdfPath)
            fileInfo.WaitWhileIsInUse()
            Dim pdfFile As New PdfFile(pdfPath)
            If pdfFile.ContainsOwnerPassword = False Then
                Try
                    Dim pdfReader As New PdfMetadataReader(pdfPath)
                    If pdfReader.Title IsNot Nothing And
                        pdfReader.Author IsNot Nothing And
                        pdfReader.Subject IsNot Nothing Then
                        Dim notes As String = Nothing
                        Dim category As String = Nothing
                        Dim taxYear As String = Nothing
                        Dim flag As Integer = 0
                        Dim suppDataHelper As New PdfSupplementalDataHelper(pdfPath)
                        Dim suppData As PdfSupplementalData = suppDataHelper.Read
                        If suppData IsNot Nothing Then
                            notes = suppData.Notes
                            category = suppData.Category
                            taxYear = suppData.TaxYear
                            flag = suppData.FlagState
                        End If
                        Using repository As IDocumentRepository =
                            New DocumentRepository
                            repository.CreateRecord(pdfReader.Title,
                                                    pdfReader.Author,
                                                    pdfReader.Subject,
                                                    pdfReader.Keywords,
                                                    notes,
                                                    pdfPath,
                                                    category,
                                                    flag,
                                                    taxYear,
                                                    pdfFile.GetTextAnnotations,
                                                    pdfFile.GetText)
                        End Using
                        IO.File.Delete(pdfPath)
                        Dim suppDataXmlPath As String =
                            IO.Path.ChangeExtension(pdfPath, "xml")
                        IO.File.Delete(suppDataXmlPath)
                        If flag = 1 Then
                            s_FlaggedDocumentsUploaded = True
                        End If
                    End If
                Catch ex As BadPasswordException    ' Ignore the file.            
                End Try
            End If
        Next
    End Sub
#End Region

End Class
