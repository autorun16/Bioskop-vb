Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports MySql.Data.MySqlClient

Public Class Index
    ' Pemanggilan Class Koneksi
    Private koneksi As New Koneksi()

    Public Shared JumlahSeat As Integer = 0
    Public Shared SelectedFilm As Integer = 0
    Public KursiTerpilih As New List(Of String)

    Public selectedMenus As New List(Of Tuple(Of Integer, String, Integer, Integer)) ' Menyimpan ID, Nama, Harga, dan Jumlah

    Private IdUser As Integer = Login.IdUser
    Private NamaUser As String = Login.UserLogin
    Private Harga As Integer = 0
    Private subTotalKonsumsi As Integer = 0
    Private Total As Integer = 0

    Private Sub Index_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AutoLayout()
        FillData()
        cntrTiket.Value = JumlahSeat
        stripUser.Text = NamaUser
    End Sub

    Private Sub btnLogout_Click(sender As Object, e As EventArgs) Handles btnLogout.Click
        Dim response
        response = MsgBox("Apakah Anda yakin ingin keluar?", vbYesNo, "Warning")
        If response = vbYes Then
            Close()
            Login.Show()
        End If
    End Sub

    Private Sub FillData()
        Try
            koneksi.OpenConn()

            Dim query As String = "SELECT * FROM film"
            Dim cmd As New MySqlCommand(query, koneksi.conn)

            Using adapter As New MySqlDataAdapter(cmd)
                Dim dataTable As New DataTable()
                adapter.Fill(dataTable)
                boxFilm.DataSource = dataTable
                boxFilm.DisplayMember = "judul"
                boxFilm.ValueMember = "id"
            End Using
        Catch ex As Exception
            MsgBox(ex.Message, vbCritical, "Error")
        End Try
    End Sub

    Private Sub btnKursi_Click(sender As Object, e As EventArgs) Handles btnKursi.Click
        If cntrTiket.Value > 0 Then
            Dim dialogForm As New SeatSelector()
            Dim result As DialogResult = dialogForm.ShowDialog()

            If result = DialogResult.OK Then
                lblKursi.Text = String.Join(", ", KursiTerpilih)
                Total = cntrTiket.Value * Harga
                lblTotal.Text = "Rp" & Total.ToString("N0")
            End If
        Else
            MsgBox("Isi Jumlah Kursi yang ingin dibeli!", vbCritical, "Error")
        End If
    End Sub

    Private Sub boxFilm_SelectedIndexChanged(sender As Object, e As EventArgs) Handles boxFilm.SelectedIndexChanged
        Try
            koneksi.OpenConn()

            Dim query As String = "SELECT * FROM film WHERE id = @idfilm"
            Dim cmd As New MySqlCommand(query, koneksi.conn)
            cmd.Parameters.AddWithValue("@idfilm", boxFilm.SelectedValue.ToString())

            Dim reader As MySqlDataReader = cmd.ExecuteReader()

            If reader.Read() Then
                lblHargaTiket.Text = "Rp" & CInt(reader("harga"))
                SelectedFilm = CInt(reader("id"))
                Harga = CInt(reader("harga"))
            End If

            koneksi.CloseConn()

        Catch ex As Exception
            MsgBox(ex.Message, vbCritical, "Error")
        End Try
    End Sub

    Private Sub btnSimpan_Click(sender As Object, e As EventArgs) Handles btnSimpan.Click
        Try
            koneksi.OpenConn()

            For Each item As String In KursiTerpilih
                Dim query As String = "INSERT INTO transaksi (id_pelanggan, id_film, seat) VALUES (@id, @idfilm, @seat)"
                Dim cmd As New MySqlCommand(query, koneksi.conn)
                cmd.Parameters.AddWithValue("@id", IdUser)
                cmd.Parameters.AddWithValue("@idfilm", boxFilm.SelectedValue.ToString())
                cmd.Parameters.AddWithValue("@seat", item)
                cmd.ExecuteNonQuery()
            Next

            For Each item In selectedMenus
                Dim query As String = "INSERT INTO transaksi_konsumsi (id_konsumsi, quantity) VALUES (@idkonsumsi, @jumlah)"
                Dim cmd As New MySqlCommand(query, koneksi.conn)
                cmd.Parameters.AddWithValue("@idkonsumsi", item.Item1)
                cmd.Parameters.AddWithValue("@jumlah", item.Item4)
                cmd.ExecuteNonQuery()
            Next

            koneksi.CloseConn()

            Dim response
            response = MsgBox("Transaksi Berhasil", vbOKOnly, "Success")
            If response = vbOK Then
                lblHargaTiket.Text = ""
                lblKursi.Text = ""
                cntrTiket.Value = 0
                lblTotal.Text = ""
                lblTotalMenu.Text = ""
                DataGridView1.Rows.Clear()
            End If

        Catch ex As Exception
            MsgBox(ex.Message, vbCritical, "Error")
        End Try
    End Sub

    Private Sub AutoLayout()
        stripUser.Text = Login.UserLogin
        Label1.Left = (ClientSize.Width - Label1.Width) / 2
    End Sub

    Private Sub btnKonsumsi_Click(sender As Object, e As EventArgs) Handles btnKonsumsi.Click
        Dim dialogForm As New FoodSelector()
        Dim result As DialogResult = dialogForm.ShowDialog()

        If result = DialogResult.OK Then
            DataGridView1.Rows.Clear()
            Dim No As Integer = 1

            For Each item In selectedMenus
                Dim subtotal As Integer = item.Item4 * item.Item3

                DataGridView1.Rows.Add(No, item.Item2, "Rp" & item.Item3.ToString("N0"), item.Item4, "Rp" & subtotal.ToString("N0"))
                subTotalKonsumsi += item.Item3 * item.Item4
                lblTotalMenu.Text = "Rp" & subTotalKonsumsi.ToString("N0")
                No += 1
                lblTotal.Text = "Rp" + (Total + subTotalKonsumsi).ToString("N0")
            Next
        End If
    End Sub
End Class