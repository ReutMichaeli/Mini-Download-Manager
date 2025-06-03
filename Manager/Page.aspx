<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Page.aspx.cs" Inherits="Manager.Page" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Mini Download Manager</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
</head>
<body class="bg-light">
    <form id="form1" runat="server">
        <div class="container mt-5">
            <div class="card shadow-sm mx-auto" style="max-width: 500px;">
                <div class="card-body text-center">
                    <h2 class="card-title mb-3 text-primary">Mini-Download Manager</h2>

                    <asp:Label ID="lblTitle" runat="server" CssClass="form-label d-block mb-2 text-muted" Text="Title will appear here" />

                    <asp:Image ID="imgIcon" runat="server" CssClass="img-thumbnail mb-3" Width="300px" />

                    <div class="d-grid gap-2 d-md-flex justify-content-md-center mb-3">
                        <asp:Button ID="btnDownload" runat="server" Text="Download" CssClass="btn btn-primary" OnClick="btnDownload_Click" />
                        <asp:Button ID="btnRefresh" runat="server" Text="Refresh" CssClass="btn btn-outline-secondary" OnClick="btnRefresh_Click" />
                    </div>

                    <asp:Label ID="lblStatus" runat="server" CssClass="text-success fw-semibold d-block" Text="" />

                    <div class="progress mt-3" style="height: 20px;">
                        <asp:Literal ID="ltProgressBar" runat="server" />
                    </div>

                    
                   


                </div>
            </div>
        </div>
    </form>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>
