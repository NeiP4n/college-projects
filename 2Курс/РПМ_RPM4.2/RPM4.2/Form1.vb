Public Class Form1
    Dim k As Integer
    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click

        Button1.Left = Rnd() * (Me.Width - Button1.Width - 30)
        Button1.Top = Rnd() * (Me.Height - Button1.Height - 20)
        k = k + 1
        Label1.Text = "Счет" + Str(k)
    End Sub
End Class
