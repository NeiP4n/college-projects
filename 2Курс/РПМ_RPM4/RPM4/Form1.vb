Public Class Form1
    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        Dim gr As Graphics = Me.CreateGraphics
        gr.FillRectangle(Brushes.Black, 100, 50, 400, 300)
        gr.DrawRectangle(Pens.White, 100, 50, 400, 300)
        gr.DrawRectangle(Pens.White, 99, 49, 402, 302)
        gr.DrawLine(Pens.White, 299, 50, 299, 350)
    End Sub
    Private Sub Button2_Click(sender As System.Object, e As System.EventArgs) Handles Button2.Click
        Dim gr As Graphics = Me.CreateGraphics
        Dim x, y As Integer
        x = Rnd() * 500 : y = Rnd() * 400
        If (x > 100) And (x < 299) And (y > 50) And (y < 348) Then
            gr.DrawEllipse(Pens.White, x, y, 3, 3)
        ElseIf (x > 300) And (x < 500) And (y > 50) And (y < 348) Then
            gr.DrawEllipse(Pens.Yellow, x, y, 3, 3)
        End If

    End Sub
End Class

