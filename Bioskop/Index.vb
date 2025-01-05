Imports System.Net.Http
Imports System.Windows.Forms.AxHost
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
    Private subTotalTiket As Integer = 0
    Private subTotalKonsumsi As Integer = 0
    Private Total As Integer = 0

    Private strukItems As New List(Of String)

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
                boxFilm.ValueMember = "idfilm"
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
                subTotalTiket = cntrTiket.Value * Harga

                If subTotalKonsumsi > 0 Then
                    lblTotal.Text = "Rp" & (subTotalTiket + subTotalKonsumsi).ToString("N0")
                Else
                    lblTotal.Text = "Rp" & subTotalTiket.ToString("N0")
                End If
            End If
        Else
            MsgBox("Isi Jumlah Kursi yang ingin dibeli!", vbCritical, "Error")
        End If
    End Sub

    Private Sub boxFilm_SelectedIndexChanged(sender As Object, e As EventArgs) Handles boxFilm.SelectedIndexChanged
        Try
            koneksi.OpenConn()

            Dim query As String = "SELECT * FROM film WHERE idfilm = @idfilm"
            Dim cmd As New MySqlCommand(query, koneksi.conn)
            cmd.Parameters.AddWithValue("@idfilm", boxFilm.SelectedValue.ToString())

            Dim reader As MySqlDataReader = cmd.ExecuteReader()

            If reader.Read() Then
                lblHargaTiket.Text = "Rp" & CInt(reader("harga"))
                SelectedFilm = CInt(reader("idfilm"))
                Harga = CInt(reader("harga"))
            End If

            koneksi.CloseConn()

        Catch ex As Exception
            MsgBox(ex.Message, vbCritical, "Error")
        End Try
    End Sub

    Private Sub btnSimpan_Click(sender As Object, e As EventArgs) Handles btnSimpan.Click
        Try
            If comboMetode.SelectedIndex > 0 Then
                koneksi.OpenConn()
                Total = subTotalTiket + subTotalKonsumsi

                Dim qTransaksi As String = "INSERT INTO transaksi_utama (idkasir, total, metode) VALUES (@idkasir, @total, @metode)"
                Dim cmdTransaksi As New MySqlCommand(qTransaksi, koneksi.conn)
                cmdTransaksi.Parameters.AddWithValue("@idkasir", IdUser)
                cmdTransaksi.Parameters.AddWithValue("@total", Total)
                cmdTransaksi.Parameters.AddWithValue("@metode", comboMetode.SelectedItem.ToString())
                cmdTransaksi.ExecuteNonQuery()

                Dim idTransaksi As Integer = cmdTransaksi.LastInsertedId

                For Each item As String In KursiTerpilih
                    Dim query As String = "INSERT INTO transaksi_tiket (idtransaksi, idkasir, idfilm, seat, subtotal) VALUES (@idtransaksi, @idkasir, @idfilm, @seat, @subtotal)"
                    Dim cmd As New MySqlCommand(query, koneksi.conn)
                    cmd.Parameters.AddWithValue("@idtransaksi", idTransaksi)
                    cmd.Parameters.AddWithValue("@idkasir", IdUser)
                    cmd.Parameters.AddWithValue("@idfilm", boxFilm.SelectedValue.ToString())
                    cmd.Parameters.AddWithValue("@seat", item)
                    cmd.Parameters.AddWithValue("@subtotal", subTotalTiket)
                    cmd.ExecuteNonQuery()
                Next

                For Each item In selectedMenus
                    Dim query As String = "INSERT INTO transaksi_konsumsi (idtransaksi, idkasir, idkonsumsi, quantity, subtotal) VALUES (@idtransaksi, @idkasir, @idkonsumsi, @jumlah, @subtotal)"
                    Dim cmd As New MySqlCommand(query, koneksi.conn)
                    cmd.Parameters.AddWithValue("@idtransaksi", idTransaksi)
                    cmd.Parameters.AddWithValue("@idkasir", IdUser)
                    cmd.Parameters.AddWithValue("@idkonsumsi", item.Item1)
                    cmd.Parameters.AddWithValue("@jumlah", item.Item4)
                    cmd.Parameters.AddWithValue("@subtotal", subTotalKonsumsi)
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
                    comboMetode.SelectedIndex = 0
                    DataGridView1.Rows.Clear()
                End If
            Else
                MsgBox("Pilih Metode Pembayaran", vbCritical, "Error")
            End If

        Catch ex As Exception
            MsgBox(ex.Message, vbCritical, "Error")
        End Try

        ' Tampilkan dialog pratinjau cetakan

        PrintPreviewDialog1.Document = PrintDocument1
        PrintPreviewDialog1.ShowDialog()
    End Sub

    Private Sub AutoLayout()
        stripUser.Text = Login.UserLogin
        Label1.Left = (ClientSize.Width - Label1.Width) / 2
        comboMetode.SelectedIndex = 0

        PrintDocument1.DefaultPageSettings.PaperSize = New Printing.PaperSize("A5", 400, 500)
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
                lblTotal.Text = "Rp" + (subTotalTiket + subTotalKonsumsi).ToString("N0")
            Next
        End If
    End Sub

    Private Sub PrintDocument1_PrintPage(sender As Object, e As Printing.PrintPageEventArgs) Handles PrintDocument1.PrintPage
        Dim font As New Font("Arial", 10)
        Dim fontBold As New Font("Arial", 12, FontStyle.Bold)
        Dim yPosition As Integer = 20 ' Awal posisi vertikal
        Dim startX As Integer = 50 ' Posisi awal X
        Dim currentX As Integer = startX

        Dim cellHeight As Integer = 30 ' Tinggi tiap sel
        Dim colWidths As Integer() = {140, 90, 90} ' Lebar kolom
        Dim headers As String() = {"Item", "Qty", "SubTotal"}

        ' Header Struk
        Dim namaBioskop As String = "BIOSKOP MEIRAYAN"
        Dim alamatBioskop As String = "ANJAS MALL Lt.3"
        Dim tanggal As String = String.Format("Tanggal: {0}", DateTime.Now.ToString("dd/MM/yyyy HH:mm"))

        Dim sizeNamaBioskop As SizeF = e.Graphics.MeasureString(namaBioskop, fontBold)
        Dim sizeAlamatBioskop As SizeF = e.Graphics.MeasureString(alamatBioskop, font)
        Dim sizeTanggal As SizeF = e.Graphics.MeasureString(tanggal, font)

        e.Graphics.DrawString(namaBioskop, fontBold, Brushes.Black, (e.PageBounds.Width - sizeNamaBioskop.Width) / 2, yPosition)
        yPosition += 30
        e.Graphics.DrawString(alamatBioskop, font, Brushes.Black, (e.PageBounds.Width - sizeAlamatBioskop.Width) / 2, yPosition)
        yPosition += 20
        e.Graphics.DrawString(tanggal, font, Brushes.Black, (e.PageBounds.Width - sizeTanggal.Width) / 2, yPosition)
        yPosition += 30

        ' Garis pemisah
        e.Graphics.DrawLine(Pens.Black, 50, yPosition, 350, yPosition)
        yPosition += 30

        ' Body Struk
        For i As Integer = 0 To headers.Length - 1
            e.Graphics.DrawString(headers(i), font, Brushes.Black, currentX, yPosition + 5)
            currentX += colWidths(i)
        Next

        yPosition += 30

        If KursiTerpilih.Count > 0 Then
            e.Graphics.DrawString(boxFilm.Text.ToString(), font, Brushes.Black, 50, yPosition)
            e.Graphics.DrawString("x" & cntrTiket.Value.ToString(), font, Brushes.Black, 190, yPosition)
            e.Graphics.DrawString("Rp" & subTotalTiket.ToString("N0"), font, Brushes.Black, 280, yPosition)
            yPosition += 30

            e.Graphics.DrawString(String.Join(", ", KursiTerpilih), font, Brushes.Black, 70, yPosition)
            yPosition += 50
        End If

        If selectedMenus.Count > 0 Then
            For Each item In selectedMenus
                Dim subtotal As Integer = item.Item4 * item.Item3
                e.Graphics.DrawString(item.Item2, font, Brushes.Black, 50, yPosition)
                e.Graphics.DrawString("x" & item.Item4.ToString(), font, Brushes.Black, 190, yPosition)
                e.Graphics.DrawString("Rp" & (item.Item3 * item.Item4).ToString("N0"), font, Brushes.Black, 280, yPosition)
                yPosition += 20
                e.Graphics.DrawString("(Rp" & item.Item3.ToString("N0") & ")", font, Brushes.Black, 70, yPosition)
                yPosition += 20
            Next
            yPosition += 30
        End If

    End Sub
End Class