Imports System.Reflection.Emit

Public Class Form1
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Label1.Visible = False
        Select Case InputBox("Имя?")

            Case ("Коля")
                Label1.Visible = True
                Label1.Text = "Привет"
                Me.BackColor = Color.YellowGreen
            Case ("Вася")
                Label1.Visible = True
                Label1.Text = "Здорово"
                Me.BackColor = Color.Green
            Case ("Джон")
                Label1.Visible = True
                Label1.Text = "Hi"
                Me.BackColor = Color.Aquamarine
            Case Else
                Label1.Visible = True
                Label1.Text = "Здраствуйте"
                Me.BackColor = Color.Red

        End Select
    End Sub

    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles Label1.Click

    End Sub
End Class
