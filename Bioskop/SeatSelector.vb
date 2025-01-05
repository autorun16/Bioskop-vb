Imports MySql.Data.MySqlClient

Public Class SeatSelector
    ' Pemanggilan Class Koneksi
    Private koneksi As New Koneksi()

    Private kursi As New List(Of String)
    Private Const baris As Integer = 6 ' Jumlah baris
    Private Const kolom As Integer = 10 ' Jumlah kolom

    Private Sub SeatSelector_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CreateSeat()
    End Sub

    Private Sub Kursi_CheckedChanged(sender As Object, e As EventArgs)
        Dim chk As CheckBox = CType(sender, CheckBox)
        Dim JumlahTiket As Integer = Index.cntrTiket.Value

        If chk.Checked Then
            If kursi.Count >= JumlahTiket Then
                chk.Checked = False
                MessageBox.Show("Anda hanya dapat memilih " & JumlahTiket & " kursi.")
            Else
                kursi.Add(chk.Tag) ' Tambahkan nama kursi ke list
            End If
        Else
            kursi.Remove(chk.Tag) ' Hapus nama kursi dari list
        End If

        UpdateKursiTerpilih()
    End Sub

    Private Sub CreateSeat()
        ' Buat checkbox untuk setiap kursi
        For i As Integer = 0 To baris - 1
            For j As Integer = 0 To kolom - 1
                Dim k = j
                If j = 5 Then
                    k += 1
                End If

                Try
                    Dim chk As New CheckBox()
                    chk.Name = Chr(Asc("A") + i) & (j + 1).ToString()
                    chk.Tag = Chr(Asc("A") + i) & (j + 1).ToString()

                    koneksi.OpenConn()
                    Dim query As String = "SELECT * FROM transaksi_tiket WHERE idfilm = @idfilm AND seat = @seat"
                    Dim cmd As New MySqlCommand(query, koneksi.conn)
                    cmd.Parameters.AddWithValue("@idfilm", Index.SelectedFilm.ToString())
                    cmd.Parameters.AddWithValue("@seat", chk.Tag)

                    Dim reader As MySqlDataReader = cmd.ExecuteReader()

                    If reader.Read() Then
                        chk.Enabled = False
                        chk.CheckState = CheckState.Indeterminate
                    End If

                    chk.Anchor = CType((((AnchorStyles.Top Or AnchorStyles.Bottom) Or AnchorStyles.Left) _
                        Or AnchorStyles.Right), AnchorStyles)
                    chk.AutoSize = True
                    chk.CheckAlign = ContentAlignment.MiddleCenter
                    chk.Font = New Font("Segoe UI", 12.0!, FontStyle.Regular, GraphicsUnit.Point, CType(0, Byte))
                    AddHandler chk.CheckedChanged, AddressOf Kursi_CheckedChanged
                    TableLayoutPanel1.Controls.Add(chk, k + 1, i + 1)

                    koneksi.CloseConn()
                Catch ex As Exception
                    MsgBox(ex.Message, vbCritical, "Error")
                End Try
            Next
        Next
    End Sub

    Private Sub UpdateKursiTerpilih()
        lblPilihan.Text = "Kursi Terpilih: " & String.Join(", ", kursi)
    End Sub

    Private Sub btnPilih_Click(sender As Object, e As EventArgs) Handles btnPilih.Click
        Index.KursiTerpilih = kursi
        DialogResult = DialogResult.OK
    End Sub
End Class