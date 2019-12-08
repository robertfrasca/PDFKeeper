﻿'******************************************************************************
'* PDFKeeper -- Open Source PDF Document Management
'* Copyright (C) 2009-2019  Robert F. Frasca
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
Public NotInheritable Class OracleDbDocumentRepository
    Implements IDocumentRepository, IDisposable
    Private provider As New OracleDbDataProvider

    Public Function GetAllAuthors() As DataTable Implements IDocumentRepository.GetAllAuthors
        Dim sqlStatement As String = _
            "select doc_author,count(doc_author) " & _
            "from pdfkeeper.docs " & _
            "group by doc_author"
        Using oraCommand As New OracleCommand(sqlStatement, provider.Connection)
            Return provider.QueryToDataTable(oraCommand)
        End Using
    End Function

    Public Function GetAllSubjects() As DataTable Implements IDocumentRepository.GetAllSubjects
        Dim sqlStatement As String = _
            "select doc_subject,count(doc_subject) " & _
            "from pdfkeeper.docs " & _
            "group by doc_subject"
        Using oraCommand As New OracleCommand(sqlStatement, provider.Connection)
            Return provider.QueryToDataTable(oraCommand)
        End Using
    End Function

    Public Function GetAllSubjectsByAuthor(author As String) As DataTable Implements IDocumentRepository.GetAllSubjectsByAuthor
        Dim sqlStatement As String = _
            "select doc_subject " & _
            "from pdfkeeper.docs " & _
            "where doc_author = :doc_author " & _
            "group by doc_subject"
        Using oraCommand As New OracleCommand(sqlStatement, provider.Connection)
            oraCommand.BindByName = True
            oraCommand.Parameters.Add("doc_author", author)
            Return provider.QueryToDataTable(oraCommand)
        End Using
    End Function

    Public Function GetAllCategories() As DataTable Implements IDocumentRepository.GetAllCategories
        Dim sqlStatement As String = _
            "select doc_category,count(doc_category) " & _
            "from pdfkeeper.docs " & _
            "group by doc_category having count(doc_category) > 0"
        Using oraCommand As New OracleCommand(sqlStatement, provider.Connection)
            Return provider.QueryToDataTable(oraCommand)
        End Using
    End Function

    Public Function GetAllRecordsBySearchString(searchValue As String) As DataTable Implements IDocumentRepository.GetAllRecordsBySearchString
        Dim sqlStatement As String = _
            "select doc_id,doc_title,doc_author,doc_subject,doc_category,doc_added " & _
            "from pdfkeeper.docs " & _
            "where (contains(doc_dummy,:doc_dummy))>0"
        Using oraCommand As New OracleCommand(sqlStatement, provider.Connection)
            oraCommand.BindByName = True
            oraCommand.Parameters.Add("doc_dummy", searchValue)
            Return provider.QueryToDataTable(oraCommand)
        End Using
    End Function

    Public Function GetAllRecordsByAuthor(author As String) As DataTable Implements IDocumentRepository.GetAllRecordsByAuthor
        Dim sqlStatement As String = _
            "select doc_id,doc_title,doc_author,doc_subject,doc_category,doc_added " & _
            "from pdfkeeper.docs " & _
            "where doc_author = :doc_author"
        Using oraCommand As New OracleCommand(sqlStatement, provider.Connection)
            oraCommand.BindByName = True
            oraCommand.Parameters.Add("doc_author", author)
            Return provider.QueryToDataTable(oraCommand)
        End Using
    End Function

    Public Function GetAllRecordsBySubject(subject As String) As DataTable Implements IDocumentRepository.GetAllRecordsBySubject
        Dim sqlStatement As String = _
            "select doc_id,doc_title,doc_author,doc_subject,doc_category,doc_added " & _
            "from pdfkeeper.docs " & _
            "where doc_subject = :doc_subject"
        Using oraCommand As New OracleCommand(sqlStatement, provider.Connection)
            oraCommand.BindByName = True
            oraCommand.Parameters.Add("doc_subject", subject)
            Return provider.QueryToDataTable(oraCommand)
        End Using
    End Function

    Public Function GetAllRecordsByAuthorAndSubject(author As String, subject As String) As DataTable Implements IDocumentRepository.GetAllRecordsByAuthorAndSubject
        Dim sqlStatement As String = _
            "select doc_id,doc_title,doc_author,doc_subject,doc_category,doc_added " & _
            "from pdfkeeper.docs " & _
            "where doc_author = :doc_author and doc_subject = :doc_subject"
        Using oraCommand As New OracleCommand(sqlStatement, provider.Connection)
            oraCommand.BindByName = True
            oraCommand.Parameters.Add("doc_author", author)
            oraCommand.Parameters.Add("doc_subject", subject)
            Return provider.QueryToDataTable(oraCommand)
        End Using
    End Function

    Public Function GetAllRecordsByCategory(category As String) As DataTable Implements IDocumentRepository.GetAllRecordsByCategory
        Dim sqlStatement As String = _
            "select doc_id,doc_title,doc_author,doc_subject,doc_category,doc_added " & _
            "from pdfkeeper.docs " & _
            "where doc_category = :doc_category"
        Using oraCommand As New OracleCommand(sqlStatement, provider.Connection)
            oraCommand.BindByName = True
            oraCommand.Parameters.Add("doc_category", category)
            Return provider.QueryToDataTable(oraCommand)
        End Using
    End Function

    Public Function GetAllRecordsByDateAdded(dateAdded As String) As DataTable Implements IDocumentRepository.GetAllRecordsByDateAdded
        Dim sqlStatement As String = _
            "select doc_id,doc_title,doc_author,doc_subject,doc_category,doc_added " & _
            "from pdfkeeper.docs " & _
            "where doc_added like :doc_added || '%'"
        Using oraCommand As New OracleCommand(sqlStatement, provider.Connection)
            oraCommand.BindByName = True
            oraCommand.Parameters.Add("doc_added", dateAdded)
            Return provider.QueryToDataTable(oraCommand)
        End Using
    End Function

    Public Function GetTotalRecordsCount() As Integer Implements IDocumentRepository.GetTotalRecordsCount
        Dim sqlStatement As String = "select count(*) from pdfkeeper.docs"
        Using oraCommand As New OracleCommand(sqlStatement, provider.Connection)
            Return provider.QueryToObject(oraCommand)
        End Using
    End Function

    Public Function GetFlaggedRecordsCount() As Integer Implements IDocumentRepository.GetFlaggedRecordsCount
        Dim sqlStatement As String = _
            "select count(doc_flag) " & _
            "from pdfkeeper.docs " & _
            "where doc_flag = 1"
        Using oraCommand As New OracleCommand(sqlStatement, provider.Connection)
            Return provider.QueryToObject(oraCommand)
        End Using
    End Function

    Public Function GetAllRecords() As DataTable Implements IDocumentRepository.GetAllRecords
        Dim sqlStatement As String = _
            "select doc_id,doc_title,doc_author,doc_subject,doc_category,doc_added " & _
            "from pdfkeeper.docs"
        Using oraCommand As New OracleCommand(sqlStatement, provider.Connection)
            Return provider.QueryToDataTable(oraCommand)
        End Using
    End Function

    Public Function GetAllFlaggedRecords() As DataTable Implements IDocumentRepository.GetAllFlaggedRecords
        Dim sqlStatement As String = _
            "select doc_id,doc_title,doc_author,doc_subject,doc_category,doc_added " & _
            "from pdfkeeper.docs " & _
            "where doc_flag = 1"
        Using oraCommand As New OracleCommand(sqlStatement, provider.Connection)
            Return provider.QueryToDataTable(oraCommand)
        End Using
    End Function

    Public Function GetNotesById(id As Integer) As DataTable Implements IDocumentRepository.GetNotesById
        Dim sqlStatement As String = _
            "select doc_notes " & _
            "from pdfkeeper.docs " & _
            "where doc_id = :doc_id"
        Using oraCommand As New OracleCommand(sqlStatement, provider.Connection)
            oraCommand.CommandType = CommandType.Text
            oraCommand.BindByName = True
            oraCommand.Parameters.Add("doc_id", id)
            Return provider.QueryToDataTable(oraCommand)
        End Using
    End Function

    Public Function GetKeywordsById(id As Integer) As DataTable Implements IDocumentRepository.GetKeywordsById
        Dim sqlStatement As String = _
            "select doc_keywords " & _
            "from pdfkeeper.docs " & _
            "where doc_id = :doc_id"
        Using oraCommand As New OracleCommand(sqlStatement, provider.Connection)
            oraCommand.BindByName = True
            oraCommand.Parameters.Add("doc_id", id)
            Return provider.QueryToDataTable(oraCommand)
        End Using
    End Function

    Public Function GetCategoryById(id As Integer) As DataTable Implements IDocumentRepository.GetCategoryById
        Dim sqlStatement As String = _
            "select doc_category " & _
            "from pdfkeeper.docs " & _
            "where doc_id = :doc_id"
        Using oraCommand As New OracleCommand(sqlStatement, provider.Connection)
            oraCommand.BindByName = True
            oraCommand.Parameters.Add("doc_id", id)
            Return provider.QueryToDataTable(oraCommand)
        End Using
    End Function

    Public Function GetFlagStateById(id As Integer) As DataTable Implements IDocumentRepository.GetFlagStateById
        Dim sqlStatement As String = _
            "select doc_flag " & _
            "from pdfkeeper.docs " & _
            "where doc_id = :doc_id"
        Using oraCommand As New OracleCommand(sqlStatement, provider.Connection)
            oraCommand.BindByName = True
            oraCommand.Parameters.Add("doc_id", id)
            Return provider.QueryToDataTable(oraCommand)
        End Using
    End Function

    Public Sub GetPdfById(id As Integer, pdfFile As String) Implements IDocumentRepository.GetPdfById
        Dim sqlStatement As String = _
            "select doc_pdf " & _
            "from pdfkeeper.docs " & _
            "where doc_id = :doc_id"
        Using oraCommand As New OracleCommand(sqlStatement, provider.Connection)
            oraCommand.BindByName = True
            oraCommand.Parameters.Add("doc_id", id)
            provider.QueryBlobToFile(oraCommand, pdfFile)
        End Using
    End Sub

    Public Sub CreateRecord(title As String, author As String, subject As String, keywords As String, notes As String, pdfFile As String, category As String, flag As Integer) Implements IDocumentRepository.CreateRecord
        Dim sqlStatement As String = _
            " begin " & _
            " insert into pdfkeeper.docs values( " & _
            " pdfkeeper.docs_seq.NEXTVAL, " & _
            " :doc_title, " & _
            " :doc_author, " & _
            " :doc_subject, " & _
            " :doc_keywords, " & _
            " to_char(sysdate,'YYYY-MM-DD HH24:MI:SS'), " & _
            " :doc_notes, " & _
            " :doc_pdf, " & _
            " '', " & _
            " :doc_category, " & _
            " :doc_flag) ;" & _
            " end ;"
        Using oraCommand As New OracleCommand(sqlStatement, provider.Connection)
            Dim fileInfo As New FileInfo(pdfFile)
            Dim blob As Byte() = fileInfo.ToByteArray
            oraCommand.BindByName = True
            oraCommand.Parameters.Add("doc_title", title)
            oraCommand.Parameters.Add("doc_author", author)
            oraCommand.Parameters.Add("doc_subject", subject)
            oraCommand.Parameters.Add("doc_keywords", keywords)
            oraCommand.Parameters.Add("doc_notes", notes)
            oraCommand.Parameters.Add("doc_pdf", OracleDbType.Blob, blob, ParameterDirection.Input)
            oraCommand.Parameters.Add("doc_category", category)
            oraCommand.Parameters.Add("doc_flag", flag)
            provider.ExecuteNonQuery(oraCommand)
        End Using
    End Sub

    Public Sub UpdateNotesById(id As Integer, notes As String) Implements IDocumentRepository.UpdateNotesById
        Dim sqlStatement As String = _
            "update pdfkeeper.docs " & _
            "set doc_notes = :doc_notes,doc_dummy = ''" & _
            "where doc_id = :doc_id"
        Using oraCommand As New OracleCommand(sqlStatement, provider.Connection)
            oraCommand.BindByName = True
            oraCommand.Parameters.Add("doc_notes", notes)
            oraCommand.Parameters.Add("doc_id", id)
            provider.ExecuteNonQuery(oraCommand)
        End Using
    End Sub

    Public Sub UpdateCategoryById(id As Integer, category As String) Implements IDocumentRepository.UpdateCategoryById
        Dim sqlStatement As String = _
            "update pdfkeeper.docs " & _
            "set doc_category = :doc_category,doc_dummy = '' " & _
            "where doc_id = :doc_id"
        Using oraCommand As New OracleCommand(sqlStatement, provider.Connection)
            oraCommand.BindByName = True
            oraCommand.Parameters.Add("doc_category", category)
            oraCommand.Parameters.Add("doc_id", id)
            provider.ExecuteNonQuery(oraCommand)
        End Using
    End Sub

    Public Sub UpdateFlagStateById(id As Integer, flag As Integer) Implements IDocumentRepository.UpdateFlagStateById
        Dim sqlStatement As String = _
            "update pdfkeeper.docs " & _
            "set doc_flag = :doc_flag " & _
            "where doc_id = :doc_id"
        Using oraCommand As New OracleCommand(sqlStatement, provider.Connection)
            oraCommand.BindByName = True
            oraCommand.Parameters.Add("doc_flag", flag)
            oraCommand.Parameters.Add("doc_id", id)
            provider.ExecuteNonQuery(oraCommand)
        End Using
    End Sub

    Public Sub DeleteRecordById(id As Integer) Implements IDocumentRepository.DeleteRecordById
        Dim sqlStatement As String = _
            "delete from pdfkeeper.docs " & _
            "where doc_id = :doc_id"
        Using oraCommand As New OracleCommand(sqlStatement, provider.Connection)
            oraCommand.BindByName = True
            oraCommand.Parameters.Add("doc_id", id)
            provider.ExecuteNonQuery(oraCommand)
        End Using
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Public Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                provider.Dispose()
            End If
        End If
        Me.disposedValue = True
    End Sub

    Protected Overrides Sub Finalize()
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(False)
        MyBase.Finalize()
    End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class