VERSION 5.00
Object = "{F9043C88-F6F2-101A-A3C9-08002B2F49FB}#1.2#0"; "COMDLG32.OCX"
Begin VB.Form MainWindow 
   Caption         =   "Pal Mod Tools"
   ClientHeight    =   9850
   ClientLeft      =   8350
   ClientTop       =   3640
   ClientWidth     =   17650
   BeginProperty Font 
      Name            =   "宋体"
      Size            =   42
      Charset         =   134
      Weight          =   400
      Underline       =   0   'False
      Italic          =   0   'False
      Strikethrough   =   0   'False
   EndProperty
   Icon            =   "MainWindow.frx":0000
   LinkTopic       =   "Form1"
   MaxButton       =   0   'False
   ScaleHeight     =   985
   ScaleMode       =   3  'Pixel
   ScaleWidth      =   1765
   StartUpPosition =   2  '屏幕中心
   Begin VB.Frame CompiledVersion_Frame 
      Caption         =   "Compiled version"
      BeginProperty Font 
         Name            =   "宋体"
         Size            =   14
         Charset         =   134
         Weight          =   400
         Underline       =   0   'False
         Italic          =   0   'False
         Strikethrough   =   0   'False
      EndProperty
      Height          =   1690
      Left            =   3720
      TabIndex        =   13
      Top             =   3600
      Width           =   3850
      Begin VB.OptionButton CompiledVersion_WIN_Option 
         Caption         =   "Compile to PAL-WIN"
         BeginProperty Font 
            Name            =   "宋体"
            Size            =   18
            Charset         =   134
            Weight          =   400
            Underline       =   0   'False
            Italic          =   0   'False
            Strikethrough   =   0   'False
         EndProperty
         Height          =   490
         Left            =   120
         TabIndex        =   15
         Top             =   1080
         Width           =   3610
      End
      Begin VB.OptionButton CompiledVersion_DOS_Option 
         Caption         =   "Compile to PAL-DOS"
         BeginProperty Font 
            Name            =   "宋体"
            Size            =   18
            Charset         =   134
            Weight          =   400
            Underline       =   0   'False
            Italic          =   0   'False
            Strikethrough   =   0   'False
         EndProperty
         Height          =   490
         Left            =   120
         TabIndex        =   14
         Top             =   360
         Value           =   -1  'True
         Width           =   3610
      End
   End
   Begin MSComDlg.CommonDialog PathSelectorDialog 
      Left            =   0
      Top             =   0
      _ExtentX        =   847
      _ExtentY        =   847
      _Version        =   393216
   End
   Begin VB.Frame ConsoleLog_Frame 
      Caption         =   "Console Log"
      BeginProperty Font 
         Name            =   "宋体"
         Size            =   14
         Charset         =   134
         Weight          =   400
         Underline       =   0   'False
         Italic          =   0   'False
         Strikethrough   =   0   'False
      EndProperty
      Height          =   3970
      Left            =   120
      TabIndex        =   10
      Top             =   5760
      Width           =   17410
      Begin VB.TextBox ConsoleLog_TextBox 
         BeginProperty Font 
            Name            =   "宋体"
            Size            =   18
            Charset         =   134
            Weight          =   400
            Underline       =   0   'False
            Italic          =   0   'False
            Strikethrough   =   0   'False
         EndProperty
         Height          =   3490
         Left            =   120
         Locked          =   -1  'True
         MultiLine       =   -1  'True
         ScrollBars      =   2  'Vertical
         TabIndex        =   12
         Top             =   360
         Width           =   17170
      End
   End
   Begin VB.CommandButton CompileGame_Button 
      Caption         =   "Compile"
      BeginProperty Font 
         Name            =   "宋体"
         Size            =   26
         Charset         =   134
         Weight          =   400
         Underline       =   0   'False
         Italic          =   0   'False
         Strikethrough   =   0   'False
      EndProperty
      Height          =   970
      Left            =   9480
      TabIndex        =   9
      Top             =   4560
      Width           =   4450
   End
   Begin VB.CommandButton UnpackGame_Button 
      Caption         =   "Unpack"
      BeginProperty Font 
         Name            =   "宋体"
         Size            =   26
         Charset         =   134
         Weight          =   400
         Underline       =   0   'False
         Italic          =   0   'False
         Strikethrough   =   0   'False
      EndProperty
      Height          =   970
      Left            =   9480
      TabIndex        =   8
      Top             =   3360
      Width           =   4450
   End
   Begin VB.CommandButton CompiledPath_Button 
      Caption         =   "..."
      BeginProperty Font 
         Name            =   "宋体"
         Size            =   26
         Charset         =   134
         Weight          =   400
         Underline       =   0   'False
         Italic          =   0   'False
         Strikethrough   =   0   'False
      EndProperty
      Height          =   610
      Left            =   16200
      TabIndex        =   7
      Top             =   2520
      Width           =   1330
   End
   Begin VB.TextBox CompiledPath_TextBox 
      BeginProperty Font 
         Name            =   "宋体"
         Size            =   26
         Charset         =   134
         Weight          =   400
         Underline       =   0   'False
         Italic          =   0   'False
         Strikethrough   =   0   'False
      EndProperty
      Height          =   620
      Left            =   5880
      TabIndex        =   6
      Top             =   2520
      Width           =   10090
   End
   Begin VB.CommandButton ModPath_Button 
      Caption         =   "..."
      BeginProperty Font 
         Name            =   "宋体"
         Size            =   26
         Charset         =   134
         Weight          =   400
         Underline       =   0   'False
         Italic          =   0   'False
         Strikethrough   =   0   'False
      EndProperty
      Height          =   610
      Left            =   16200
      TabIndex        =   4
      Top             =   1440
      Width           =   1330
   End
   Begin VB.TextBox ModPath_TextBox 
      BeginProperty Font 
         Name            =   "宋体"
         Size            =   26
         Charset         =   134
         Weight          =   400
         Underline       =   0   'False
         Italic          =   0   'False
         Strikethrough   =   0   'False
      EndProperty
      Height          =   620
      Left            =   5880
      TabIndex        =   3
      Top             =   1440
      Width           =   10090
   End
   Begin VB.CommandButton GamePath_Button 
      Caption         =   "..."
      BeginProperty Font 
         Name            =   "宋体"
         Size            =   26
         Charset         =   134
         Weight          =   400
         Underline       =   0   'False
         Italic          =   0   'False
         Strikethrough   =   0   'False
      EndProperty
      Height          =   610
      Left            =   16200
      TabIndex        =   1
      Top             =   360
      Width           =   1330
   End
   Begin VB.TextBox GamePath_TextBox 
      BeginProperty Font 
         Name            =   "宋体"
         Size            =   26
         Charset         =   134
         Weight          =   400
         Underline       =   0   'False
         Italic          =   0   'False
         Strikethrough   =   0   'False
      EndProperty
      Height          =   620
      Left            =   5880
      TabIndex        =   0
      Top             =   360
      Width           =   10090
   End
   Begin VB.Label GamePath_Label 
      Alignment       =   1  'Right Justify
      AutoSize        =   -1  'True
      Caption         =   "Game Path"
      Height          =   850
      Left            =   1800
      TabIndex        =   11
      Top             =   240
      Width           =   3850
   End
   Begin VB.Label CompiledPath_Label 
      Alignment       =   1  'Right Justify
      AutoSize        =   -1  'True
      Caption         =   "Compiled Path"
      Height          =   850
      Left            =   120
      TabIndex        =   5
      Top             =   2400
      Width           =   5530
   End
   Begin VB.Label ModPath_Label 
      Alignment       =   1  'Right Justify
      AutoSize        =   -1  'True
      Caption         =   "Mod Path"
      Height          =   850
      Left            =   2280
      TabIndex        =   2
      Top             =   1320
      Width           =   3370
   End
End
Attribute VB_Name = "MainWindow"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Option Explicit

Private Function OpenPath(title As String, textBox As textBox)
    Dim objShell As Object
    Dim objFolder As Object
    Dim folderPath As String
    
    MainWindow.Hide
    
    '
    ' 创建 Shell.Application 对象
    '
    Set objShell = CreateObject("Shell.Application")
    
    '
    ' 弹出文件夹选择对话框（&H1 表示禁止选择系统虚拟文件夹）
    '
    Set objFolder = objShell.BrowseForFolder(0, title, &H1, ".\")
    
    '
    ' 检查用户是否选择了文件夹（而非点击取消）
    '
    If Not objFolder Is Nothing Then
        textBox.Text = objFolder.Self.path
    End If
    
    '
    ' 释放对象
    '
    Set objFolder = Nothing
    Set objShell = Nothing
    
    MainWindow.Show
End Function

Private Sub GamePath_Button_Click()
    OpenPath "Select the game path:", GamePath_TextBox
End Sub

Private Sub ModPath_Button_Click()
    OpenPath "Select the unpacking path for the game mod:", ModPath_TextBox
End Sub

Private Sub CompiledPath_Button_Click()
    OpenPath "Select the path of the game mod compilation result:", CompiledPath_TextBox
End Sub
