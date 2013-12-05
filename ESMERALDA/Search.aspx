<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Search.aspx.cs" Inherits="ESMERALDA.Search"
    ValidateRequest="false" %>

    <%@ Register Assembly="SlimeeLibrary" Namespace="SlimeeLibrary" TagPrefix="cc1" %>   
    <%@ register assembly="GMaps" namespace="Subgurim.Controles" tagprefix="gmap" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">    
    <link href="css/style.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <link href="css/tooltip.css?t=<%= DateTime.Now.Ticks %>" type="text/css" rel="stylesheet" />
    <script src="scripts/esmeralda.js" type="text/javascript" language="javascript"></script>
    <script src="https://maps.googleapis.com/maps/api/js?sensor=false&libraries=drawing"></script>
    <script src="scripts/tooltip.js" language="javascript" type="text/javascript"></script>
    <style type="text/css">
    v\:* {
        behavior:url(#default#VML);
    }
    </style>
    <script language="javascript" type="text/javascript">

        var map;
        var infowindow;
        var shapes;

        // Sets the map on all markers in the array.
        function setAllMap(map) {
            for (var i = 0; i < polys.length; i++) {
                polys[i].setMap(map);
            }
        }

        // Removes the markers from the map, but keeps them in the array.
        function clearMarkers() {
            setAllMap(null);
        }

        function open_in_new_tab(url) {
            window.open(url, '_blank');
            window.focus();
        }

        /*function initializeMap() {
            fixMapSize();
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
        }*/

        /*function initializeMap() {
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
            drawingManager.setMap(subgurim_map1);
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
            google.maps.event.addListener(drawingManager, 'overlaycomplete', executeShape);
        }*/

        function setDataNotes(content) {
            var el = document.getElementById("datanotes_content");
            el.innerHTML = content;
        }

        /*function attachInfoWindow(overlay, incon, coords, link) {
            google.maps.event.addListener(overlay, 'mouseover', function () { setDataNotes(incon); });
            google.maps.event.addListener(overlay, 'mouseout', function () { setDataNotes(" "); });
            google.maps.event.addListener(overlay, 'click', function () { window.open(link, "_blank"); });
        }*/

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

        /*function fixMapSize() {
            var wrapper = document.getElementById('map_wrapper');
            var width = wrapper.clientWidth;
            var map = document.getElementById('map_canvas');
            map.style.width = width;
            map.style.height = width;
        }*/

        function initializeAll() {
            initalizeParent();
            initializeMap();
        }

    </script>
</head>
<body onload='initializeAll()'>
    <form id="form1" runat="server">
    <div id="page_wrapper">
        <div id="pagecontent">
            <div id="searchbykeyword">
                <h3>
                    Search</h3>
                <p>
                    Search for datasets by keyword, date, or geographic boundaries. Type in your keywords separated by spaces.</p>                
                <table border="0">
                    <tr><td colspan="4">Enter your keywords:</td></tr>
                    <tr><td colspan="4"><asp:TextBox ID="txtSearchByKeyword" runat="server" Width="469px"></asp:TextBox></td></tr>
                    <tr><td colspan="4">Enter your date range (if desired):</td></tr>
                    <tr>
                        <td>Starting date:</td>
                        <td>
                            <asp:TextBox ID="txtStartDate" runat="server"></asp:TextBox>
                        </td>
                        <td>Ending date:</td>
                        <td>
                            <asp:TextBox ID="txtEndDate" runat="server"></asp:TextBox>
                        </td>
                    </tr>
                    <tr><td colspan="4">Enter your geographic bounds (if desired):</td></tr>
                    <tr><td>Minimum Latitude:</td><td>
                        <asp:TextBox ID="txtMinLatitude" runat="server"></asp:TextBox></td><td>Minimum Longitude</td><td><asp:TextBox ID="txtMinLongitude" runat="server"></asp:TextBox></td></tr>
                    <tr><td>Maximum Latitude:</td><td><asp:TextBox ID="txtMaxLatitude" runat="server"></asp:TextBox></td><td>Maximum Longitude</td><td><asp:TextBox ID="txtMaxLongitude" runat="server"></asp:TextBox></td></tr>
                    <tr><td colspan="4"><asp:LinkButton ID="btnSearchByKeyword" runat="server" CssClass="squarebutton"><span>Search</span></asp:LinkButton></td></tr>
                </table>                               
                <asp:Table ID="tblSearchByKeywordResults" runat="server">
                </asp:Table>
            </div>
            <div id="searchgeospatial" style="width:100%">
                <h3>
                    Geospatial Search</h3>
                <p>
                    If you would like to limit your search to a specific geographic area, use the rectangle
                    tool in the map below to draw the bounds of your search area. You can also click
                    on datasets below to open them.</p>
                <table border="0" style="width:100%">
                    <tr>
                        <td width="50%">
                            <gmap:GMap ID="map1" runat="server" Width="600px" Height="600px" enableGetGMapElementById="true" enableServerEvents="true" enableHookMouseWheelToZoom="true"></gmap:GMap>
                        </td>
                        <td width="50%">
                            <div id="datanotes"><span id="datanotes_content"></span></div>
                        </td>
                    </tr>
                </table>
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
