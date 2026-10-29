Public Class Form1
    Private GraphicsFun As System.Drawing.Graphics
    Private BrushColor As New SolidBrush(Color.Yellow)
    Dim i, x0, y0, x, y As Integer
    Dim k, b As Double
    Dim korX(99), korY(99) As Integer
    Dim urav(99, 99) As Double
    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        PictureBox1.Refresh()
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        PictureBox1.Refresh()
        Randomize()
        GraphicsFun = PictureBox1.CreateGraphics
        x0 = PictureBox1.Width / 2
        y0 = PictureBox1.Height / 2
        For i As Integer = 0 To 20
            korX(i) = Rnd() * PictureBox1.Width
            korY(i) = Rnd() * PictureBox1.Height
            GraphicsFun.FillEllipse(BrushColor, korX(i), korY(i), 5, 5)
        Next i
    End Sub
    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        PictureBox1.Refresh()
        For i As Integer = 0 To 20
            x = korX(i)
            y = korY(i)
            If (korX(i) <> x0) Or (korY(i) <> y0) Then
                If x > x0 Then
                    x = x - 1
                    y = urav(i, i) * x + urav(i, i + 1)     ''Уравнение прямой y=kx+b
                    GraphicsFun.FillEllipse(BrushColor, x, y, 5, 5)
                    korX(i) = x
                    korY(i) = y
                End If
                If x < x0 Then
                    x = x + 1
                    y = urav(i, i) * x + urav(i, i + 1)     ''Уравнение прямой y=kx+b
                    GraphicsFun.FillEllipse(BrushColor, x, y, 5, 5)
                    korX(i) = x
                    korY(i) = y
                End If
            Else
                GraphicsFun.FillEllipse(BrushColor, x, y, 5, 5) ''Звезда остается в центре
                '' Если закоментировать эту команду,то звезды будут "проваливаться" в черную дыру
            End If
        Next
    End Sub
    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        For i As Integer = 0 To 20
            x = x0 - korX(i)
            y = y0 - korY(i)
            k = y / x
            b = y0 - x0 * k
            urav(i, i) = k
            urav(i, i + 1) = b
        Next i
        x = 0
        y = 0
        Timer1.Start()
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        Timer1.Stop()
    End Sub
     
End Class
