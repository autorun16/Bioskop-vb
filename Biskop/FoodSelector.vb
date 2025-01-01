Imports MySql.Data.MySqlClient
Imports Mysqlx.Notice

Public Class FoodSelector
    Private koneksi As New Koneksi()
    Private menuItems As New Dictionary(Of CheckBox, MenuItem)
    Private y As Integer = 70 ' Posisi awal vertikal
    Private tabIndex As Integer = 1

    Private Sub FoodSelector_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CreateMenu()
    End Sub

    Private Sub CreateMenu()
        Try
            Dim kategori As New List(Of String)

            ' 1. Ambil Kategori
            koneksi.OpenConn()
            Dim query As String = "SELECT DISTINCT jenis FROM konsumsi"
            Dim cmd As New MySqlCommand(query, koneksi.conn)

            Using readerKategori As MySqlDataReader = cmd.ExecuteReader()
                While readerKategori.Read()
                    kategori.Add(readerKategori("jenis").ToString())
                End While
            End Using

            ' 2. Tampilkan checkbox berdasarkan kategori

            For Each jenis As String In kategori
                Dim lblKategori As New Label()
                Dim panelLeft As Integer = 0

                lblKategori.Text = jenis
                lblKategori.Location = New Point(20, y)
                Controls.Add(lblKategori)

                y += lblKategori.Height + 5

                Using cmdItem As New MySqlCommand("SELECT * FROM konsumsi WHERE jenis = @kategori", koneksi.conn)
                    cmdItem.Parameters.AddWithValue("@kategori", jenis)

                    Using readerItem As MySqlDataReader = cmdItem.ExecuteReader()
                        While readerItem.Read()
                            Dim panelItem As New TableLayoutPanel()
                            panelItem.Height = 60
                            panelItem.Location = New Point(0, y)
                            'panelItem.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
                            panelItem.RowCount = 2
                            panelItem.RowStyles.Add(New RowStyle(SizeType.Percent, 60.0!))
                            panelItem.RowStyles.Add(New RowStyle(SizeType.Percent, 40.0!))
                            panelItem.ColumnCount = 2
                            panelItem.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 70.0!))
                            panelItem.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 30.0!))
                            'panelItem.Dock = DockStyle.Top
                            panelItem.Left = (ClientSize.Width - panelItem.Width) / 2

                            Dim lblNama As New Label()
                            lblNama.Text = readerItem("nama").ToString()
                            lblNama.Font = New Font("Segoe UI", 10.8!, FontStyle.Regular, GraphicsUnit.Point, CType(0, Byte))
                            lblNama.TextAlign = ContentAlignment.MiddleLeft
                            lblNama.Dock = DockStyle.Fill

                            Dim lblHarga As New Label()
                            lblHarga.Text = "Rp" & readerItem("harga").ToString
                            lblHarga.Font = New Font("Segoe UI", 9.0!, FontStyle.Regular, GraphicsUnit.Point, CType(0, Byte))
                            lblHarga.TextAlign = ContentAlignment.TopLeft
                            lblHarga.Dock = DockStyle.Fill

                            Dim qtyItem As New NumericUpDown()
                            qtyItem.Minimum = 0
                            qtyItem.Maximum = 100
                            qtyItem.Tag = New Tuple(Of Integer, Integer)(readerItem("id"), readerItem("harga")) ' Menyimpan ID makanan di properti Tag
                            qtyItem.Font = New Font("Segoe UI", 10.8!, FontStyle.Regular, GraphicsUnit.Point, CType(0, Byte))
                            qtyItem.TextAlign = HorizontalAlignment.Center
                            qtyItem.TabIndex = tabIndex
                            qtyItem.Dock = DockStyle.Fill

                            panelItem.Controls.Add(lblNama, 0, 0)
                            panelItem.Controls.Add(lblHarga, 0, 1)
                            panelItem.Controls.Add(qtyItem, 1, 0)
                            'panelItem.SetRowSpan(qtyItem, 2)
                            Controls.Add(panelItem)

                            panelLeft = (panelItem.Width - lblKategori.Width) / 2

                            tabIndex += 1
                            y += panelItem.Height + 5
                        End While
                    End Using
                End Using
                lblKategori.Left = panelLeft
                y += 10
            Next

            TableLayoutPanel1.Location = New Point((ClientSize.Width - TableLayoutPanel1.Width) / 2, y)
            btnBack.TabIndex = tabIndex + 1
            btnConfirm.TabIndex = tabIndex + 2

        Catch ex As Exception
            MsgBox(ex.Message, vbCritical, "Error")
        Finally
            koneksi.CloseConn()
        End Try
    End Sub

    Private Sub Menu_CheckedChanged(sender As Object, e As EventArgs)

    End Sub

    Private Sub UpdateMenu()

    End Sub

    Private Sub AutoLayout()
        Label1.Left = (ClientSize.Width - Label1.Width) / 2
        'TableLayoutPanel1.Left = (ClientSize.Width - TableLayoutPanel1.Width) / 2
        'FlowLayoutPanel1.Left = (ClientSize.Width - FlowLayoutPanel1.Width) / 2
    End Sub

    Private Sub btnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click
        DialogResult = DialogResult.Cancel
    End Sub

    Private Sub btnConfirm_Click(sender As Object, e As EventArgs) Handles btnConfirm.Click
        Index.selectedMenus.Clear()

        For Each panel As TableLayoutPanel In Me.Controls.OfType(Of TableLayoutPanel)()
            Dim qty As NumericUpDown = panel.Controls.OfType(Of NumericUpDown).FirstOrDefault()
            Dim lblNama As Label = panel.Controls.OfType(Of Label).FirstOrDefault()

            If qty IsNot Nothing Then
                Dim tagData = CType(qty.Tag, Tuple(Of Integer, Integer))
                Dim idMenu As Integer = tagData.Item1
                Dim nama As String = lblNama.Text.ToString()
                Dim harga As Integer = tagData.Item2
                Dim jumlah As Integer = CInt(qty.Value)

                If jumlah > 0 Then
                    Index.selectedMenus.Add(New Tuple(Of Integer, String, Integer, Integer)(idMenu, nama, harga, jumlah))
                End If
            End If
        Next

        DialogResult = DialogResult.OK
    End Sub
End Class