Imports System.Security.Cryptography
Imports System.Text
Imports MySql.Data.MySqlClient

Public Class Register

    ' Pemanggilan Class Koneksi
    Private koneksi As New Koneksi()

    Private Sub Register_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AutoLayout()
    End Sub

    Private Sub btnDaftar_Click(sender As Object, e As EventArgs) Handles btnDaftar.Click
        If (txtNama.Text IsNot "" And txtEmail IsNot "" And txtUsername IsNot "" And txtPass IsNot "" And txtConfirm IsNot "") Then
            If (txtPass.Text = txtConfirm.Text) Then
                Try
                    koneksi.OpenConn()

                    Dim query As String = "INSERT INTO pelanggan (nama, email, username, pass) VALUES (@nama, @email, @username, @pass)"

                    Dim cmd As New MySqlCommand(query, koneksi.conn)
                    cmd.Parameters.AddWithValue("@nama", txtNama.Text)
                    cmd.Parameters.AddWithValue("@email", txtEmail.Text)
                    cmd.Parameters.AddWithValue("@username", txtUsername.Text)
                    cmd.Parameters.AddWithValue("@pass", Hashing(txtConfirm.Text))
                    cmd.ExecuteNonQuery()
                    koneksi.CloseConn()

                    Dim Response = MsgBox("Akun Berhasil di Daftarkan", vbInformation, "Success")
                    If Response = vbOK Then
                        Dispose()
                        Login.Show()
                    End If
                Catch ex As Exception
                    MsgBox(ex.Message, vbCritical, "Error")
                End Try
            Else
                MsgBox("Password Tidak Sama!", vbCritical, "Error")
            End If
        Else
            MsgBox("Mohon Isi Kolom yang KOSONG!", vbCritical, "Error")
        End If
    End Sub

    ' Enkripsi Password menjadi SHA256
    Public Function Hashing(ByVal password As String) As String
        Dim sha256 As SHA256 = SHA256.Create()
        Dim inputBytes As Byte() = Encoding.UTF8.GetBytes(password)
        Dim hashBytes As Byte() = sha256.ComputeHash(inputBytes)
        Return BitConverter.ToString(hashBytes).Replace("-", "").ToLower()
    End Function

    Private Sub AutoLayout()
        Label1.Left = (ClientSize.Width - Label1.Width) / 2

        lblNama.Location = New Point((ClientSize.Width - lblNama.Width) / 2, Label1.Bottom + 20)
        txtNama.Location = New Point((ClientSize.Width - txtNama.Width) / 2, lblNama.Bottom + 5)

        lblEmail.Location = New Point((ClientSize.Width - lblEmail.Width) / 2, txtNama.Bottom + 10)
        txtEmail.Location = New Point((ClientSize.Width - txtEmail.Width) / 2, lblEmail.Bottom + 5)

        lblUsername.Location = New Point((ClientSize.Width - lblUsername.Width) / 2, txtEmail.Bottom + 10)
        txtUsername.Location = New Point((ClientSize.Width - txtUsername.Width) / 2, lblUsername.Bottom + 5)

        lblPass.Location = New Point((ClientSize.Width - lblPass.Width) / 2, txtUsername.Bottom + 10)
        txtPass.Location = New Point((ClientSize.Width - txtPass.Width) / 2, lblPass.Bottom + 5)

        lblConfirm.Location = New Point((ClientSize.Width - lblConfirm.Width) / 2, txtPass.Bottom + 10)
        txtConfirm.Location = New Point((ClientSize.Width - txtConfirm.Width) / 2, lblConfirm.Bottom + 5)

        btnDaftar.Location = New Point((ClientSize.Width - btnDaftar.Width) / 2, txtConfirm.Bottom + 30)
        btnBack.Location = New Point((ClientSize.Width - btnBack.Width) / 2, btnDaftar.Bottom + 20)

        lblNama.Left = txtNama.Left
        lblEmail.Left = txtEmail.Left
        lblUsername.Left = txtUsername.Left
        lblPass.Left = txtPass.Left
        lblConfirm.Left = txtConfirm.Left
    End Sub

    Private Sub btnBack_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles btnBack.LinkClicked
        Dispose()
        Login.Show()
    End Sub
End Class