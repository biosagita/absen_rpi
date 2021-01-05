Imports MySql.Data.MySqlClient
Imports System.IO
Imports System.Text
Imports System.IO.Ports
Imports System.Data.SqlClient

Module koneksi
    Public conn As New MySqlConnection
    Dim WithEvents COMPort As New SerialPort
    Dim SpaceCount As Byte = 0
    Dim koneksisql As SqlConnection
    Dim cmdsql As New SqlCommand
    Dim mydasql As SqlDataAdapter
    Dim mydatasetsql As DataSet
    Dim readersql As SqlDataReader
    Dim konsql, querysql, fieldtgl, fieldname, fieldrate As String
    Public Sub connect()
        Dim strServerName As String
        Dim strDatabaseName As String
        Dim strUserName As String
        Dim strPassword As String
        Dim strport As String

        'strServerName = "localhost"
        'strServerName = "192.168.102.103"
        'strDatabaseName = "db_antrian"
        'strport = "3306"
        'strUserName = "root"
        'strPassword = ""
        'strPassword = "vertrigo"
        'strPassword = "rahasia"

        Dim init As String()
        init = File.ReadAllText("config.ini").Split(Environment.NewLine)
        '  MsgBox(init(0))
        strDatabaseName = init(0)
        strServerName = init(1)
        strport = init(2)
        strUserName = init(3)
        strPassword = init(4)
        '  konsql = init(6)
        ' querysql = init(7)

        Try
            If conn.State = ConnectionState.Closed Then
                conn.ConnectionString = "DATABASE=" & strDatabaseName _
                & ";SERVER = " & strServerName & ";PORT = " & strport _
                & ";user id=" & strUserName & ";password=" & strPassword & ";charset=utf8"
                conn.Open()
            End If
        Catch ex As MySql.Data.MySqlClient.MySqlException
            MsgBox("Koneksi Eror")
            End

        End Try


       











    End Sub
    Public Sub disconnect()
        Try
            conn.Close()
            conn.Dispose()
        Catch ex As MySql.Data.MySqlClient.MySqlException
        End Try
    End Sub

    Public Function BytesToHexString(ByVal bytes() As Byte, ByVal panjang As Integer) As String
        Dim sb As New StringBuilder(bytes.Length * 2)
        For Each b As Byte In bytes
            sb.AppendFormat("{0:X2}", b)
        Next b
        Return sb.ToString()
    End Function
    Public Function BA2String(ByVal bytearray As Byte()) As String
        '  Public Shared Function BA2String(ByRef bytearray As Byte()) As String

        Dim tmp As String = ""
        For Each b In bytearray
            If Not b = 0 Then
                tmp = tmp + Convert.ToChar(b)
            End If
        Next
        Return tmp

        'Return ASCIIEncoding.ASCII.GetString(bytearray) + ""
        'Return "lanjut"

        'Dim enc As New System.Text.UTF8Encoding()
        'Return enc.GetString(bytearray)
    End Function
 
    Public Function rubahtgl(ByVal tgl As String)
        Return Replace(Replace(Replace(Replace(Replace(Replace(Replace(tgl, "Sunday", "Minggu"), "Monday", "Senin"), "Tuesday", "Selasa"), "Wednesday", "Rabu"), "Thursday", "Kamis"), "Friday", "Jumat"), "Saturday", "Sabtu")
    End Function

    Public Function StrToHex(ByRef Data As String) As String
        Dim sVal As String
        Dim sHex As String = ""
        While Data.Length > 0
            sVal = Conversion.Hex(Strings.Asc(Data.Substring(0, 1).ToString()))
            Data = Data.Substring(1, Data.Length - 1)
            sHex = sHex & sVal
        End While
        Return sHex
    End Function


    Public Sub konek(comstring As String)
        ' MsgBox(comstring)
        Try
            COMPort.PortName = comstring
            COMPort.Open()
        Catch
            MsgBox(COMPort.PortName & " tidak bisa digunakan")
        End Try

    End Sub
    Public Sub tunggu(ByVal PMillseconds As Integer)
        ' Function created to replace thread.sleep()
        ' Provides responsive main form without using threading.

        Dim TimeNow As DateTime
        Dim TimeEnd As DateTime
        Dim StopFlag As Boolean

        TimeEnd = Now()
        TimeEnd = TimeEnd.AddMilliseconds(PMillseconds)
        StopFlag = False
        While Not StopFlag
            TimeNow = Now()
            If TimeNow > TimeEnd Then
                StopFlag = True
            End If
            Application.DoEvents()
        End While

        ' Cleanup
        TimeNow = Nothing
        TimeEnd = Nothing
    End Sub

    Public Sub transmitter(ByVal textstring As String)
        '   Received.AppendText(vbCrLf)        ' Switch to a new line after every transmission
        SpaceCount = 0
        'Dim TextString As String
        Dim TXArray(2047) As Byte
        Dim I As Integer
        Dim J As Integer = 0

        Dim Ascii As Boolean = False
        Dim Quote As Boolean = False
        Dim Temp As Boolean
        Dim Second As Boolean = False
        Dim TXByte As Byte = 0
        Dim CharByte As Byte

        If COMPort.IsOpen Then
            ' Transmitted.AppendText(textstring)
            For I = 0 To textstring.Length - 1
                CharByte = Asc(textstring.Chars(I))
                If CharByte = 34 Then ' If " Then
                    Temp = Ascii
                    Ascii = Ascii Or Quote
                    Quote = Not (Temp And Quote)
                Else
                    Ascii = Ascii Xor Quote
                    Quote = False
                End If
                If Not Quote Then
                    If Ascii Then
                        TXArray(J) = CharByte
                        J = J + 1
                    Else
                        If (CharByte <> 32) And (CharByte <> 10) And (CharByte <> 13) Then ' Skip spaces, LF and CR
                            CharByte = (CharByte - 48) And 31 ' And 31 makes it case insensitive
                            If CharByte > 16 Then
                                CharByte = CharByte - 7
                            End If
                            If Second Then
                                TXArray(J) = TXByte + CharByte
                                Second = False
                                J = J + 1
                            Else
                                TXByte = CharByte << 4
                                Second = True
                            End If
                        End If
                    End If
                End If
            Next
            Try
                COMPort.Write(TXArray, 0, J)
            Catch ex As Exception
                '  MsgBox(ex.Message & "  Check CTS signal or set Flow Control to None.")
            End Try
        Else
            'MsgBox("COM port is closed. Please select a COM port")
            'Timer1.Enabled = False
        End If


    End Sub



End Module




