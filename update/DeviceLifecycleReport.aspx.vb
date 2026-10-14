Imports System.Data
Imports DevExpress.XtraCharts

Partial Class DeviceLifecycleReport
    Inherits System.Web.UI.Page
    Dim cnstring As String = System.Configuration.ConfigurationManager.ConnectionStrings("ConnectionString").ConnectionString
    Dim client As New service
    Private Sub DeviceLifecycleReport_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            LoadDeviceInfo()
            BindChartTCO()
            BindChartHistory()
            BindGridMaintenanceLog()
        End If
    End Sub
    Private Sub LoadDeviceInfo()
        lblDeviceInfo.Text = "Thiết bị: Máy chụp Cắt lớp từ cộng hưởng (MRI) - Hãng: Siemens - Model: Magnetom Sola 1.5T - Năm SX: 2021"
        lblStatus.Text = "Trạng thái hiện tại: Đang hoạt động (Health Score: 85/100) - Lịch kiểm định tiếp theo: 15/11/2026"
        lblStatus.ForeColor = System.Drawing.Color.Green
    End Sub

    ' Tạo dữ liệu giả định cho Biểu đồ Pie (TCO)
    Private Sub BindChartTCO()
        Dim dtTCO As New DataTable()
        dtTCO.Columns.Add("Category", GetType(String))
        dtTCO.Columns.Add("Amount", GetType(Decimal)) ' Đơn vị: Triệu VNĐ

        dtTCO.Rows.Add("Chi phí mua sắm ban đầu (CapEx)", 35000)
        dtTCO.Rows.Add("Chi phí bảo trì & sửa chữa (5 năm)", 4500)
        dtTCO.Rows.Add("Chi phí vật tư & linh kiện thay thế", 3200)
        dtTCO.Rows.Add("Chi phí vận hành (Điện, làm mát từ trường)", 2100)

        chartTCO.DataSource = dtTCO
        chartTCO.DataBind()
    End Sub

    ' Tạo dữ liệu giả định cho Biểu đồ Cột & Đường (Lịch sử lỗi)
    Private Sub BindChartHistory()
        Dim dtHistory As New DataTable()
        dtHistory.Columns.Add("Year", GetType(String))
        dtHistory.Columns.Add("Breakdowns", GetType(Integer))
        dtHistory.Columns.Add("RepairCost", GetType(Decimal))

        dtHistory.Rows.Add("2022", 1, 150)
        dtHistory.Rows.Add("2023", 2, 320)
        dtHistory.Rows.Add("2024", 1, 200)
        dtHistory.Rows.Add("2025", 4, 850)
        dtHistory.Rows.Add("2026", 2, 410)

        chartHistory.DataSource = dtHistory

        Dim xyDiagram As XYDiagram = TryCast(chartHistory.Diagram, XYDiagram)
        If xyDiagram Is Nothing Then Return

        ' === Trục Y chính ===
        xyDiagram.AxisY.Title.Text = "Số lần hỏng đột xuất"
        xyDiagram.AxisY.Title.Visibility = DevExpress.Utils.DefaultBoolean.True
        xyDiagram.AxisY.WholeRange.Auto = False
        xyDiagram.AxisY.WholeRange.SetMinMaxValues(0, 5)
        xyDiagram.AxisY.NumericScaleOptions.GridSpacing = 1

        ' === Trục Y phụ ===
        Dim secondaryAxis As SecondaryAxisY
        If xyDiagram.SecondaryAxesY.Count = 0 Then
            secondaryAxis = New SecondaryAxisY("CostAxis")
            secondaryAxis.Title.Text = "Chi phí (Triệu VNĐ)"
            secondaryAxis.Title.Visibility = DevExpress.Utils.DefaultBoolean.True
            xyDiagram.SecondaryAxesY.Add(secondaryAxis)
        Else
            secondaryAxis = xyDiagram.SecondaryAxesY("CostAxis")
        End If

        secondaryAxis.WholeRange.Auto = True
        secondaryAxis.WholeRange.AlwaysShowZeroLevel = True

        ' === Gán Series sang trục phụ ===
        Dim costSeries As Series = chartHistory.Series("Chi phí sửa chữa (Triệu VNĐ)")
        If costSeries IsNot Nothing Then
            ' Đảm bảo map đúng cột dữ liệu (phòng trường hợp aspx thiếu)
            costSeries.ArgumentDataMember = "Year"
            costSeries.ValueDataMembers.Clear()
            costSeries.ValueDataMembers.AddRange(New String() {"RepairCost"})

            Dim lineView As LineSeriesView = TryCast(costSeries.View, LineSeriesView)
            If lineView IsNot Nothing Then
                lineView.AxisYName = "CostAxis"
                lineView.MarkerVisibility = DevExpress.Utils.DefaultBoolean.True
            End If

            costSeries.Visible = True          ' Bật hiển thị
            'costSeries.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True  ' Hiện giá trị trên điểm (tùy chọn)
        End If

        ' DataBind
        chartHistory.DataBind()

        ' 6. (Tùy chọn) Nếu muốn ép max đẹp hơn một chút (làm tròn lên)
        secondaryAxis.WholeRange.Auto = False
        Dim maxCost = dtHistory.AsEnumerable().Max(Function(r) r.Field(Of Decimal)("RepairCost"))
        secondaryAxis.WholeRange.SetMinMaxValues(0, Math.Ceiling(maxCost / 100) * 100) ' ví dụ làm tròn lên 100
    End Sub
    ' Tạo dữ liệu giả định cho Lưới Nhật ký bảo trì (Grid)
    Private Sub BindGridMaintenanceLog()
        Dim dtLog As New DataTable()
        dtLog.Columns.Add("Date", GetType(DateTime))
        dtLog.Columns.Add("Type", GetType(String))
        dtLog.Columns.Add("Description", GetType(String))
        dtLog.Columns.Add("Cost", GetType(Decimal))
        dtLog.Columns.Add("Engineer", GetType(String))
        dtLog.Columns.Add("Status", GetType(String))

        dtLog.Rows.Add(New DateTime(2026, 8, 10), "Bảo trì phòng ngừa", "Nạp khí Helium lỏng, kiểm tra hệ thống làm mát", 150000000, "Nguyễn Văn A (Siemens)", "Hoàn thành")
        dtLog.Rows.Add(New DateTime(2026, 5, 22), "Sửa chữa đột xuất", "Thay thế bo mạch khuếch đại RF bị lỗi", 260000000, "Trần Văn B (BV)", "Hoàn thành")
        dtLog.Rows.Add(New DateTime(2026, 2, 15), "Kiểm định", "Kiểm định an toàn bức xạ từ trường định kỳ", 15000000, "TT Kiểm định Y Tế", "Đạt")
        dtLog.Rows.Add(New DateTime(2025, 11, 5), "Bảo trì phòng ngừa", "Vệ sinh coil chụp, cập nhật phần mềm trạm làm việc", 20000000, "Nguyễn Văn A (Siemens)", "Hoàn thành")
        dtLog.Rows.Add(New DateTime(2025, 9, 12), "Sửa chữa đột xuất", "Lỗi màn hình điều khiển khoang chụp", 45000000, "Lê Văn C (BV)", "Hoàn thành")
        dtLog.Rows.Add(New DateTime(2025, 6, 30), "Bảo trì phòng ngừa", "Kiểm tra hệ thống điện nguồn UPS", 5000000, "Phạm D (BV)", "Hoàn thành")

        gridMaintenanceLog.DataSource = dtLog
        gridMaintenanceLog.DataBind()
    End Sub
End Class
