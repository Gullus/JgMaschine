Imports System.Threading
Imports System.Net

''' <summary>
''' Nord-Informatik GmbH
''' Am Flugplatz 3
''' 31137 Hildesheim
''' Fon. (05121) 91 88 4 - 33
''' Fax: (05121) 91 88 4 - 55
''' 
'''info@nord-informatik.de

''' Für die Funktionsfähigkeit für dieses Programm übernehmen wir keine Haftung oder Garantie.
''' </summary>
''' <remarks></remarks>

Public Class Form1
    Dim t As Thread
    Private Shared listener As New HttpListener
    Public Shared runServer As Boolean = True
    Delegate Sub InvokeControl(ByVal [text] As String)
    Dim isConnected As Boolean = False

    Private Sub Start()
        AsynchronousListener(New String() {"http://localhost:8080/"})
    End Sub

    Public Sub AsynchronousListener(ByVal prefixes() As String)
        Try

            'Anpassen des Listeners (Prefix)
            Try
                Dim fileContents As String
                '  fileContents = "http://" & tbIP.Text & ":" & tbPort.Text & "/xx/"
                fileContents = My.Computer.FileSystem.ReadAllText(My.Application.Info.DirectoryPath & "\config.txt")
                Debug.WriteLine(fileContents)
                listener.Prefixes.Add(fileContents)
            Catch ex As Exception

            End Try
            listener.Start()
            Dim _invokeControl As New InvokeControl(AddressOf InvokeUIThread)
            ListBox1.Invoke(_invokeControl, "Entering request processing loop")
            While runServer
                Dim result As IAsyncResult = listener.BeginGetContext(New AsyncCallback(AddressOf AsynchronousListenerCallback), listener)
                ListBox1.Invoke(_invokeControl, "Waiting for asyncronous request processing.")
                result.AsyncWaitHandle.WaitOne()
                ListBox1.Invoke(_invokeControl, "Asynchronous request processed.")
            End While
            listener.Close()


        Catch ex As Exception

        End Try
    End Sub

    Private Shared Sub InvokeUIThread(ByVal [text] As String)
        Form1.ListBox1.Items.Add([text])
    End Sub

    Public Sub AsynchronousListenerCallback(ByVal result As IAsyncResult)
        Try
            Dim listener As HttpListener = CType(result.AsyncState, HttpListener)
            Dim context As HttpListenerContext = listener.EndGetContext(result)
            Dim request As HttpListenerRequest = context.Request
            Dim response As HttpListenerResponse = context.Response
            Debug.Print(context.Request.QueryString.ToString)
            Try
                Dim s As String = ""
                Dim posCheckSum As Integer = calcChecksum(context.Request.QueryString)
                s = "status=ok&checksum=" & posCheckSum.ToString
                s &= "&" & tbAntwort.Text
                Dim responseString As String = s
                responseString &= vbCrLf
                Dim _invokeControl As New InvokeControl(AddressOf InvokeUIThread)
                ListBox1.Invoke(_invokeControl, responseString)
                Debug.WriteLine(responseString)
                Dim buffer As Byte() = System.Text.Encoding.UTF8.GetBytes(responseString)
                context.Response.ContentLength64 = buffer.Length
                Dim output As System.IO.Stream = context.Response.OutputStream
                output.Write(buffer, 0, buffer.Length)
                output.Flush()
                output.Close()
            Catch ex As Exception
         
            End Try
        Catch ex As Exception
            '  Application.Exit()
    
        End Try
    End Sub
    Private Sub Form1_FormClosing(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        runServer = False
        t.Abort()
    End Sub
    Private Function calcChecksum(ByVal queryString As System.Collections.Specialized.NameValueCollection) As Integer
        Dim sum As Integer = 0
        Try
            For Each key As String In queryString.Keys
                If key = "checksum" Then
                    Continue For
                End If
                Dim str As String = queryString(key)
                For Each ch As Char In str.ToArray
                    sum += Asc(ch)
                Next
            Next
        Catch ex As Exception

        End Try

        Return sum
    End Function

    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        t = New Thread(New ThreadStart(AddressOf Start))
        t.Start()
    End Sub
End Class
