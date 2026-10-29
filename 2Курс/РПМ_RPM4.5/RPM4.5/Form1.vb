Public Class Form1

    Private Sub Form1_Layout(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LayoutEventArgs) Handles MyBase.Layout

        Dim gr As Graphics = Me.CreateGraphics
        gr.FillRectangle(Brushes.Black, 0, 70, 10, 10)
        gr.DrawLine(Pens.Black, 0, 80, Me.Width, 80)
        gr.FillRectangle(Brushes.Blue, 280, 70, 20, 10)
        gr.FillRectangle(Brushes.Blue, 650, 70, 50, 10)
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim a As Integer = InputBox("Стреляй!")
        Dim gr As Graphics = Me.CreateGraphics
        a = a * 10

        If (a >= 280) And (a <= 300) Or (a >= 650) And (a <= 700) Then
            gr.FillEllipse(Brushes.Red, a, 76, 6, 6)
            MsgBox("Попал!")
        ElseIf (a >= 270) And (a < 280) Or (a > 300) And (a <= 310) Or (a >= 640) And
               (a < 650) Or (a > 700) And (a <= 710) Then
            gr.FillEllipse(Brushes.Red, a, 76, 6, 6)
            MsgBox("Близко!")
        ElseIf (a >= 0) And (a <= 20) Then
            gr.FillEllipse(Brushes.Red, a, 76, 6, 6)
            MsgBox("Не бей по своим!")
        ElseIf (a > 20) And (a < 270) Then
            gr.FillEllipse(Brushes.Red, a, 76, 6, 6)
            MsgBox("Не долет!")
        ElseIf (a > 310) And (a < 640) Then
            gr.FillEllipse(Brushes.Red, a, 76, 6, 6)
            MsgBox("Между целями!")
        End If
    End Sub
End Class
