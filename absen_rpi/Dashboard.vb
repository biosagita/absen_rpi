Imports MySql.Data.MySqlClient
Public Class Dashboard
    Dim cmd As MySqlCommand
    Dim reader As MySqlDataReader
    Dim dataadapter As MySqlDataAdapter
    Dim mydatatable As DataTable
    Dim device_table As DataTable
    Dim ip_device, nama_device As String
    Dim id_device, port_device, last_id_check As Integer
    Dim client(16) As MySqlConnection
    Dim jumlah_mesin As Integer
    Dim sql As String
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Dim last_id_now As Integer
        Dim tanggal_absen As String

        cmd = New MySqlCommand("select * from device", conn)
        dataadapter = New MySqlDataAdapter(cmd)
        device_table = New DataTable
        dataadapter.Fill(device_table)

        For i As Integer = 0 To device_table.Rows.Count - 1

            id_device = device_table.Rows(i).Item("id_device")
            last_id_check = device_table.Rows(i).Item("last_id_check")
            ' read id transaksi mesin terakhir
            cmd = New MySqlCommand("SELECT id FROM attendance ORDER BY id DESC LIMIT 0,1", client(i))
            last_id_now = cmd.ExecuteScalar

            'get data mesin dr id terakhir disimpan
            cmd = New MySqlCommand("select * from attendance where id>" & last_id_check, client(i))
            dataadapter = New MySqlDataAdapter(cmd)
            mydatatable = New DataTable
            dataadapter.Fill(mydatatable)

            'input data yg didapat ke database aplikasi
            For Each row As DataRow In mydatatable.Rows
                tanggal_absen = Format(row.Item("clock_in"), "yyyy-MM-dd HH:mm:ss")
                cmd = New MySqlCommand("insert into transaction(tanggal, user_code, machine) 
                values('" & tanggal_absen & "', '" & row.Item("user_id") & "', '" & id_device & "')", conn)
                cmd.ExecuteNonQuery()
            Next
            'update last_id_check
            cmd = New MySqlCommand("update device set last_id_check=" & last_id_now & " where id_device = " & id_device, conn)
            cmd.ExecuteNonQuery()

        Next
        ' MsgBox("keluar loop device")
        refresh_datagrid()
    End Sub

    Private Sub Dashboard_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        connect()
        cmd = New MySqlCommand("select * from device", conn)
        dataadapter = New MySqlDataAdapter(cmd)
        device_table = New DataTable
        dataadapter.Fill(device_table)

        jumlah_mesin = 0
        For Each row As DataRow In device_table.Rows
            client(jumlah_mesin) = New MySqlConnection
            Try
                Dim konstring As String = "DATABASE=attendance_system;
                    SERVER = " & row.Item("ip_device") & ";PORT = 3306; 
                    user id=pi;password=raspberry;charset=utf8"
                ' MsgBox(konstring)
                If client(jumlah_mesin).State = ConnectionState.Closed Then
                    client(jumlah_mesin).ConnectionString = konstring
                    client(jumlah_mesin).Open()
                End If
            Catch ex As MySqlException
                MsgBox("Koneksi " & nama_device & ex.Message)
                End

            End Try
            jumlah_mesin += 1
        Next
        ' MsgBox(nama_device)
    End Sub

    Sub refresh_datagrid()
        Dim sql As String
        sql = "SELECT TIME(a.tanggal), b.user_name, c.nama_device  
            FROM transaction a
            LEFT JOIN user b ON a.user_code = b.user_code
            LEFT JOIN device c ON a.machine = c.id_device 
            where date(tanggal) = '" & Format(Date.Now, "yyyy-MM-dd") & "'"
        ' MsgBox(sql)
        cmd = New MySqlCommand(sql, conn)
        dataadapter = New MySqlDataAdapter(cmd)
        mydatatable = New DataTable
        dataadapter.Fill(mydatatable)
        DataGridView1.DataSource = mydatatable
    End Sub
End Class
