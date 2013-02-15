<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Search.aspx.cs" Inherits="ESMERALDA.Search" ValidateRequest="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <meta name="viewport" content="initial-scale=1.0, user-scalable=no">
    <title>Search ESMERALDA</title>
    <link href="css/style.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <link href="css/tooltip.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <script src="scripts/esmeralda.js" type="text/javascript" language="javascript"></script>
    <script src="https://maps.googleapis.com/maps/api/js?sensor=false&libraries=drawing"></script>
    <script src="scripts/tooltip.js" language="javascript" type="text/javascript"></script>
    <script language="javascript" type="text/javascript">

        var map;
        var infowindow;
        var shapes;

        function open_in_new_tab(url) {
            window.open(url, '_blank');
            window.focus();
        }

        function initializeMap() {
            shapes = new Array();
            var myLatLng = new google.maps.LatLng(24.886436490787712, -70.2685546875);
            var zoomval = 1;
            var tokens = null;
            var subtokens = null;
            var el = document.getElementById('mapZoom');
            if (el.value && el.value.length > 0) {
                zoomval = parseInt(el.value);
            }
            el = document.getElementById('mapCenter');
            if (el.value && el.value.length > 0) {
                tokens = el.value.split(",");
                myLatLng = new google.maps.LatLng(parseFloat(tokens[0]), parseFloat(tokens[1]));
            }
            var mapOptions = {
                zoom: zoomval,
                center: myLatLng,
                mapTypeId: google.maps.MapTypeId.TERRAIN
            };
            map = new google.maps.Map(document.getElementById('map_canvas'), mapOptions);
            var drawingManager = new google.maps.drawing.DrawingManager({
                drawingControl: true,
                drawingControlOptions: {
                    position: google.maps.ControlPosition.TOP_CENTER,
                    drawingModes: [google.maps.drawing.OverlayType.RECTANGLE]
                },
                circleOptions: {
                    fillColor: '#ffff00',
                    fillOpacity: 1,
                    strokeWeight: 5,
                    clickable: false,
                    zIndex: 1,
                    editable: true
                }
            });
            drawingManager.setMap(map);
            var polystring = document.getElementById("searchCoords").value;
            if (polystring && polystring.length > 0) {
                subtokens = polystring.split(" ");
                var bermudaTriangle;
                // Construct the polygon
                bermudaTriangle = new google.maps.Rectangle({
                    strokeOpacity: 0.8,
                    strokeWeight: 2,
                    fillOpacity: 0.35
                });
                var latLngBounds = new google.maps.LatLngBounds(
                new google.maps.LatLng(subtokens[2], subtokens[3]),
                  new google.maps.LatLng(subtokens[0], subtokens[1])
                );
                bermudaTriangle.setBounds(latLngBounds);
                bermudaTriangle.setMap(map);
            }
            var datasets = document.getElementById("mapdatasets").value;
            if (datasets.length > 0) {
                var setlines = datasets.split("|");
                var i = 0;
                for (i = 0; i < setlines.length; i++) {
                    tokens = setlines[i].split("~");
                    subtokens = tokens[3].split(";");
                    var j = 0;
                    var latLngBound = new Array();
                    for (j = 0; j < subtokens.length; j++) {
                        var subsubtokens = subtokens[j].split(",");
                        latLngBound.push(new google.maps.LatLng(subsubtokens[0], subsubtokens[1]));
                    }
                    // Construct the polygon
                    shapes.push(new google.maps.Polygon({
                        paths: latLngBound,
                        strokeOpacity: 0.8,
                        strokeWeight: 2,
                        fillOpacity: 0.35,
                        strokeColor: tokens[0],
                        fillColor: tokens[0],
                        clickable: true
                    }));
                    shapes[i].setMap(map);
                    attachInfoWindow(shapes[i], tokens[1], latLngBound[0], tokens[2]);
                }
            }
            google.maps.event.addListener(drawingManager, 'overlaycomplete', executeShape);
        }

        function attachInfoWindow(overlay, incon, coords, link) {
            var infowindow = new google.maps.InfoWindow({ content: incon });
            infowindow.setPosition(coords);
            google.maps.event.addListener(overlay, 'mouseover', function () { infowindow.open(map); });
            google.maps.event.addListener(overlay, 'mouseout', function () { infowindow.close(map); });
            google.maps.event.addListener(overlay, 'click', function () { window.open(link, "_blank"); });
        }

        function executeShape(event) {
            if (event.type == google.maps.drawing.OverlayType.RECTANGLE) {
                var zoom = map.getZoom();

                var el = document.getElementById('mapZoom');
                el.value = zoom;
                el = document.getElementById('mapCenter');
                el.value = map.getCenter().toUrlValue();

                var path = event.overlay.getBounds();
                var path_string = path.getNorthEast().lat() + " " + path.getNorthEast().lng() + " " + path.getSouthWest().lat() + " " + path.getSouthWest().lng();
                var control = document.getElementById('searchCoords');
                control.value = path_string;
                event.overlay.setMap(null);
                form1.submit();
            }
        }
    </script>
</head>
<body onload='initalizeParent()'>
    <form id="form1" runat="server">
    <div id="page_wrapper">
        <div id="pagecontent">
            <div id="searchbykeyword">
                <h3>
                    Keyword Search</h3>
                <p>
                    Search for datasets by keyword (searches metadata and field names).  Type in your keywords separated by spaces.</p>
                Enter your keywords:
                <asp:TextBox ID="txtSearchByKeyword" runat="server" Width="469px"></asp:TextBox>
                <asp:LinkButton ID="btnSearchByKeyword" runat="server">Search</asp:LinkButton>
                <asp:Table ID="tblSearchByKeywordResults" runat="server">
                </asp:Table>
            </div>
            <div id="searchgeospatial">
                <h3>
                    Geospatial Search</h3>
                    <p>If you would like to limit your search to a specific geographic area, use the rectangle tool in the map below to draw the bounds of your search area.  You can also click on datasets below to open them.</p>
                <div id="map_canvas" style="width: 600px; height: 600px">
                </div>
                <asp:HiddenField ID="mapdatasets" runat="server" />
                <asp:HiddenField ID="searchCoords" runat="server" />
                <asp:HiddenField ID="mapCenter" runat="server" />
                <asp:HiddenField ID="mapZoom" runat="server" />
            </div>
            <div id="externalSearchResults">
            <h3>
                    Results from Other Sites</h3>
                <asp:Table ID="tblExternalSearchResults" runat="server">
                </asp:Table>
            </div>
        </div>
    </div>
    </form>
</body>
</html>