Imports MySql.Data.MySqlClient

Public Class Koneksi
    Public conn As MySqlConnection

    Public Sub OpenConn()
        Dim str As String = "server=localhost;userid=root;password=;database=bioskop"

        Try
            conn = New MySqlConnection(str)
            conn.Open()
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Public Sub CloseConn()
        If conn.State = ConnectionState.Open Then
            conn.Close()
        End If
    End Sub
End Class
