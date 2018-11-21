Imports System.IO

Public Class KAnalyse
    Public Property Streams As New Dictionary(Of FileInfo, sg)
    Public Property File As FileInfo
    Dim c As New Kmer_Sort
    Public Property Header As String
    Public Property FileNames As List(Of String)

    Public Sub New(Files As List(Of FileInfo), File As FileInfo)
        Dim Index As Integer = 1
        For Each F In Files
            Streams.Add(F, New sg(F, Index))
            Index += 1
        Next
        Me.File = File
        FileNames = (From x In Files Select x.Name).ToList
        Header = "Kmer" & vbTab & Szunyi.Common.Text.General.GetText(FileNames, vbTab)

    End Sub
    Public Sub DoIt()
        Dim KMers = ParseAll()
        ' Check Same Size
        Dim dCounts = (From x In KMers Select x.Seq.Length).Distinct.ToList
        If dCounts.Count <> 1 Then
            MsgBox("Different Kmer size detected. Run is aborted")
            Exit Sub
        End If
        Using sw As New StreamWriter(Me.File.FullName, True)
            sw.Write(Header)
            Do
                KMers.Sort(c)
                Dim Done As Boolean = False
                If KMers.Count = 1 Then
                    sw.WriteLine()
                    sw.Write(Get_Current_Counts(KMers, KMers.Count - 1))
                    KMers.Clear()
                    KMers = ParseAll()
                Else
                    For i1 = 0 To KMers.Count - 1
                        If KMers(0).Seq <> KMers(i1).Seq Then
                            sw.WriteLine()
                            sw.Write(Get_Current_Counts(KMers, i1 - 1))
                            ' Write to Files
                            ' Get NewOnes
                            For i = 0 To i1 - 1
                                Dim t = Streams(KMers(i).File).ParseOne()
                                If IsNothing(t) = False Then KMers.Add(t)
                            Next
                            KMers.RemoveRange(0, i1)
                            Done = True
                            Exit For
                        End If
                    Next
                    If Done = False Then
                        sw.WriteLine()
                        sw.Write(Get_Current_Counts(KMers, KMers.Count - 1))
                        KMers.Clear()
                        KMers = ParseAll()
                    End If
                End If

            Loop Until KMers.Count = 0
        End Using

    End Sub
    Private Function Get_Current_Counts(Kmers As List(Of Kmer), Index As Integer) As String
        Dim t(Me.FileNames.Count) As String
        t(0) = Kmers(0).Seq
        For i1 = 1 To FileNames.Count
            t(i1) = 0
        Next

        For i1 = 0 To Index
            Dim j = Me.Streams(Kmers(i1).File).Index
            t(j) = Kmers(i1).Count
        Next
        Return Szunyi.Common.Text.General.GetText(t, vbTab)
    End Function
    Public Function ParseAll() As List(Of Kmer)
        Dim Out As New List(Of Kmer)
        For Each s In Streams
            Dim t = s.Value.ParseOne
            If IsNothing(t) = False Then Out.Add(t)
        Next
        Return Out
    End Function

    Public Class sg
        Public Property File As FileInfo
        Public Property Stream As FileStream
        Public Property Index As Integer
        Private t_Reader As TextReader
        Public Sub New(File As FileInfo, Index As Integer)

            t_Reader = File.OpenText
            Me.Index = Index
            Me.File = File
        End Sub
        Public Function ParseOne() As Kmer
            Try
                Return New Kmer(t_Reader.ReadLine, File)

            Catch ex As Exception
                Dim j As Int16 = 43
            End Try

            Return Nothing
        End Function

    End Class

End Class

''' <summary>
''' Basic class hold information of single Kmer :Seq,Count and FileInfo
''' </summary>
Public Class Kmer
    Public Property Seq As String
    Public Property Count As Integer
    Public Property File As FileInfo
    Public Sub New(Line As String, File As FileInfo)
        Me.File = File
        Dim s = Split(Line, vbTab)
        Me.Seq = s.First
        Me.Count = s.Last
    End Sub
End Class

''' <summary>
''' Sorting Based on Seq
''' </summary>
Public Class Kmer_Sort
    Implements IComparer(Of Kmer)
    Public Function Compare(x As Kmer, y As Kmer) As Integer Implements IComparer(Of Kmer).Compare
        Return x.Seq.CompareTo(y.Seq)
    End Function
End Class

Public Class KAnalyseFilterSetting
    Public Property MinSumInAll As Szunyi.IO.Outer_Programs.Input_Description
    Public Property MinSumInGroup As Szunyi.IO.Outer_Programs.Input_Description
    Public Property MinPositiveInGroup As Szunyi.IO.Outer_Programs.Input_Description
    Public Property MinPercentOfGroup As Szunyi.IO.Outer_Programs.Input_Description
    Public Sub New()
        MinSumInAll = Szunyi.IO.Parameters.Get_Integer("Min Sum of Kmer in All Files", 10, 1, Integer.MaxValue)
        MinSumInGroup = Szunyi.IO.Parameters.Get_Integer("Min Sum of Kmer in Group Files", 10, 1, Integer.MaxValue)
        MinPositiveInGroup = Szunyi.IO.Parameters.Get_Integer("Min nof positive Files in Group Files", 10, 1, Integer.MaxValue)
        MinPercentOfGroup = Szunyi.IO.Parameters.Get_Integer("Min Percent of Kmer in a Group", 99, 1, 100)

    End Sub
End Class
Public Class Filter

End Class

