﻿'******************************************************************************
'* PDFKeeper -- Open Source PDF Document Storage Solution
'* Copyright (C) 2009-2018 Robert F. Frasca
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
Public Class OracleDataProvider
    Implements IDisposable, IDataProvider
    Private credential As DatabaseCredential = DatabaseCredential.Instance
    Private connection As New OracleConnection

    Public Sub TestConnection() Implements IDataProvider.TestConnection
        OpenConnection()
        CloseConnection()
    End Sub

    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", _
        "CA2100:Review SQL queries for security vulnerabilities")> _
    Public Function ExecuteQuery(sqlStatement As String) As DataTable Implements IDataProvider.ExecuteQuery
        Using adapter As New OracleDataAdapter(sqlStatement, connection)
            Using table As New DataTable
                table.Locale = CultureInfo.InvariantCulture
                OpenConnection()
                adapter.Fill(table)
                CloseConnection()
                Return table
            End Using
        End Using
    End Function

    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", _
        "CA2100:Review SQL queries for security vulnerabilities")> _
    Public Function ExecuteScalarQuery(sqlStatement As String) As Object Implements IDataProvider.ExecuteScalarQuery
        Using Command As New OracleCommand(sqlStatement, connection)
            OpenConnection()
            Return Command.ExecuteScalar
            CloseConnection()
        End Using
    End Function

    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", _
        "CA2100:Review SQL queries for security vulnerabilities")> _
    Public Sub ExecuteBlobQuery(sqlStatement As String, _
                                targetPathName As String) Implements IDataProvider.ExecuteBlobQuery
        Using command As New OracleCommand(sqlStatement, connection)
            OpenConnection()
            Using dataReader As OracleDataReader = command.ExecuteReader
                dataReader.Read()
                Dim blob As OracleBlob = dataReader.GetOracleBlob(0)
                Using memoryStream As New MemoryStream(blob.Value)
                    Using fileStream As New FileStream(targetPathName, FileMode.Create, FileAccess.Write)
                        fileStream.Write(memoryStream.ToArray, 0, CInt(blob.Length))
                    End Using
                End Using
            End Using
            CloseConnection()
        End Using
    End Sub

    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", _
        "CA2100:Review SQL queries for security vulnerabilities")> _
    Public Sub ExecuteBlobInsert(sqlStatement As String, _
                                 sourcePathName As String) Implements IDataProvider.ExecuteBlobInsert
        Using command As New OracleCommand(sqlStatement, connection)
            Dim blob As Byte() = FileHelper.GetFileAsByteArray(sourcePathName)
            OpenConnection()

            ' Bind the parameter to the insert statement.
            command.CommandType = CommandType.Text
            Dim parameter As OracleParameter = command.Parameters.Add("doc_pdf", _
                                                                      OracleDbType.Blob)
            parameter.Direction = ParameterDirection.Input
            parameter.Value = blob

            command.ExecuteNonQuery()
            CloseConnection()
        End Using
    End Sub

    <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", _
        "CA2100:Review SQL queries for security vulnerabilities")> _
    Public Sub ExecuteNonQuery(sqlStatement As String) Implements IDataProvider.ExecuteNonQuery
        Using command As New OracleCommand(sqlStatement, connection)
            OpenConnection()
            command.ExecuteNonQuery()
            CloseConnection()
        End Using
    End Sub

    Private Sub OpenConnection()
        connection.ConnectionString = _
            "User Id=" + credential.UserName + ";" & _
            "Password=" + credential.Password.ToPlainTextString + ";" & _
            "Data Source=" + credential.DataSource + ";" & _
            "Persist Security Info=False;Pooling=True"
        connection.Open()
    End Sub

    Private Sub CloseConnection()
        Dispose()
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                connection.Close()
                connection.Dispose()
            End If
        End If
        Me.disposedValue = True
    End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class