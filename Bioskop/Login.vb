Imports System.Security.Cryptography
Imports System.Text
Imports MySql.Data.MySqlClient

Public Class Login
    ' Pemanggilan Class Koneksi
    Private koneksi As New Koneksi()
    Public Shared UserLogin As String
    Public Shared IdUser As Integer

    Private Sub btnDaftar_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles btnDaftar.LinkClicked
        Register.Show()
        Hide()
    End Sub

    Private Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        If txtUsername.Text IsNot "" And txtPass.Text IsNot "" Then
            Try
                koneksi.OpenConn()
                Dim query As String = "SELECT * FROM pelanggan WHERE email = @user OR username = @user AND pass = @pass"

                Dim cmd As New MySqlCommand(query, koneksi.conn)
                cmd.Parameters.AddWithValue("@user", txtUsername.Text)
                cmd.Parameters.AddWithValue("@pass", Hashing(txtPass.Text))

                Dim reader As MySqlDataReader = cmd.ExecuteReader()

                If reader.Read() Then
                    'MsgBox("Login Berhasil!", vbInformation, "Success")
                    IdUser = CInt(reader("id"))
                    UserLogin = reader("nama").ToString()
                    Hide()
                    Index.Show()
                Else
                    MsgBox("User Tidak di Temukan!", vbCritical, "Error")
                End If

                koneksi.CloseConn()
            Catch ex As Exception
                MsgBox(ex.Message, vbCritical, "Error")
            End Try
        Else
            MsgBox("Mohon Isi Kolom Yang KOSONG!", vbCritical, "Error")
        End If
    End Sub

    ' Enkripsi Password menjadi SHA256
    Public Function Hashing(ByVal password As String) As String
        Dim sha256 As SHA256 = sha256.Create()
        Dim inputBytes As Byte() = Encoding.UTF8.GetBytes(password)
        Dim hashBytes As Byte() = sha256.ComputeHash(inputBytes)
        Return BitConverter.ToString(hashBytes).Replace("-", "").ToLower()
    End Function

    Private Sub Login_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Label1.Left = (ClientSize.Width - Label1.Width) / 2
        txtUsername.Left = (ClientSize.Width - txtUsername.Width) / 2
        txtPass.Left = (ClientSize.Width - txtPass.Width) / 2
        btnLogin.Left = (ClientSize.Width - btnLogin.Width) / 2
        Label2.Left = txtUsername.Left
        Label3.Left = txtPass.Left
        btnDaftar.Left = (ClientSize.Width - btnDaftar.Width) / 2
    End Sub
End Class
