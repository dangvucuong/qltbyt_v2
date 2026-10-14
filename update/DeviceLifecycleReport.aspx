<%@ Page Language="VB" AutoEventWireup="false" MasterPageFile="~/main.master" CodeFile="DeviceLifecycleReport.aspx.vb" Inherits="DeviceLifecycleReport" %>

<%@ Register Assembly="DevExpress.XtraCharts.v23.1.Web, Version=23.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.XtraCharts.Web" TagPrefix="dx" %>
<%@ Register assembly="DevExpress.XtraCharts.v23.1, Version=23.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" namespace="DevExpress.XtraCharts" tagprefix="dx" %>

<%@ Register Assembly="DevExpress.Web.v23.1, Version=23.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.XtraCharts.v23.1.Web, Version=23.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.XtraCharts.Web" TagPrefix="dxchartsui" %>
<%@ Register Assembly="DevExpress.XtraCharts.v23.1, Version=23.1.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.XtraCharts" TagPrefix="dxcharts" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <style>
        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f6f9; margin: 0; padding: 20px; }
        .header-panel { background: #fff; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); margin-bottom: 20px; }
        .title { font-size: 24px; font-weight: bold; color: #2c3e50; }
        .subtitle { font-size: 14px; color: #7f8c8d; margin-top: 5px; }
        .dashboard-container { display: flex; flex-wrap: wrap; gap: 20px; margin-bottom: 20px; }
        .chart-box { background: #fff; padding: 15px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); flex: 1; min-width: 400px; }
        .grid-box { background: #fff; padding: 15px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
    </style>
    <script>
        $(document).ready(function () {
        })
    </script><!-- Thông tin chung -->
        <div class="header-panel">
            <h3>Báo Cáo Phân Tích Vòng Đời Thiết Bị</h3>
            <hr />
            <div class="subtitle">
                <dx:ASPxLabel ID="lblDeviceInfo" runat="server" Font-Bold="true" ForeColor="#2980b9" />
            </div>
            <div style="margin-top: 10px;">
                <dx:ASPxLabel ID="lblStatus" runat="server" Font-Bold="true" />
            </div>
        </div>

        <!-- Vùng chứa Biểu đồ -->
        <div class="dashboard-container">
            <!-- Biểu đồ TCO (Pie Chart) -->
            <div class="chart-box">
                <h3 style="color:#34495e; margin-top:0;">1. Cơ Cấu Tổng Chi Phí Sở Hữu</h3>
                <dxchartsui:WebChartControl ID="chartTCO" runat="server" Height="800px" Width="480px">
                    <SeriesSerializable>
                        <dxcharts:Series Name="TCO_Series" ArgumentDataMember="Category" ValueDataMembersSerializable="Amount">
                            <ViewSerializable>
                                <dxcharts:PieSeriesView></dxcharts:PieSeriesView>
                            </ViewSerializable>
                            <LabelSerializable>
                                <dxcharts:PieSeriesLabel TextPattern="{A}: {V:N0} Triệu VNĐ ({VP:P0})" Position="TwoColumns"></dxcharts:PieSeriesLabel>
                            </LabelSerializable>
                        </dxcharts:Series>
                    </SeriesSerializable>
                    <Legend AlignmentHorizontal="Center" AlignmentVertical="BottomOutside"></Legend>
                </dxchartsui:WebChartControl>
            </div>

            <!-- Biểu đồ Tần suất lỗi & Chi phí (Bar & Line Chart) -->
            <div class="chart-box">
                <h3 style="color:#34495e; margin-top:0;">2. Tần Suất Lỗi & Chi Phí Sửa Chữa</h3>
                <dxchartsui:WebChartControl ID="chartHistory" runat="server" Height="800px" Width="480px">
                    <SeriesSerializable>
                        <%-- Series cột (trục Y chính) --%>
                        <dx:Series Name="Số lần hỏng đột xuất" 
                                   ArgumentDataMember="Year" 
                                   ValueDataMembersSerializable="Breakdowns">
                            <ViewSerializable>
                                <dx:SideBySideBarSeriesView />
                            </ViewSerializable>
                        </dx:Series>

                        <%-- Series đường (trục Y phụ) --%>
                        <dx:Series Name="Chi phí sửa chữa (Triệu VNĐ)" 
                                   ArgumentDataMember="Year" 
                                   ValueDataMembersSerializable="RepairCost">
                            <ViewSerializable>
                                <dx:LineSeriesView MarkerVisibility="True" />
                            </ViewSerializable>
                        </dx:Series>
                    </SeriesSerializable>
                    <Legend AlignmentHorizontal="Right" AlignmentVertical="TopOutside"></Legend>
                </dxchartsui:WebChartControl>
            </div>
        </div>

        <!-- Lưới dữ liệu nhật ký bảo trì -->
        <div class="grid-box">
            <h3 style="color:#34495e; margin-top:0;">3. Nhật Ký Bảo Trì & Sửa Chữa</h3>
            <dx:ASPxGridView ID="gridMaintenanceLog" runat="server" AutoGenerateColumns="False" Width="100%" Theme="Material">
                <Columns>
                    <dx:GridViewDataDateColumn FieldName="Date" Caption="Ngày Thực Hiện" Width="120px">
                        <PropertiesDateEdit DisplayFormatString="dd/MM/yyyy" />
                    </dx:GridViewDataDateColumn>
                    <dx:GridViewDataTextColumn FieldName="Type" Caption="Loại Hình" Width="150px" />
                    <dx:GridViewDataTextColumn FieldName="Description" Caption="Mô Tả Chi Tiết" />
                    <dx:GridViewDataSpinEditColumn FieldName="Cost" Caption="Chi Phí (VNĐ)" Width="150px">
                        <PropertiesSpinEdit DisplayFormatString="N0" />
                    </dx:GridViewDataSpinEditColumn>
                    <dx:GridViewDataTextColumn FieldName="Engineer" Caption="Kỹ Sư Phụ Trách" Width="150px" />
                    <dx:GridViewDataTextColumn FieldName="Status" Caption="Trạng Thái" Width="120px" />
                </Columns>
                <Settings ShowFilterRow="True" ShowGroupPanel="False" />
                <SettingsPager PageSize="10" />
            </dx:ASPxGridView>
        </div>
</asp:Content>
