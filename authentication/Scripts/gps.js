function GetLocation() {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(LocationSuccess, LocationError);
    } else {
        LocationError('not supported');
    }
}

function LocationSuccess(position) {
    setLatLongTextBoxes(position.coords.latitude, position.coords.longitude);
    var myLatlng = new google.maps.LatLng(position.coords.latitude, position.coords.longitude);
}

function LocationError() {
    alert("Unable to locate you.");
}

function setLatLongTextBoxes(theLat, theLong) {
    $('#Latitude').val(theLat);
    $('#Longitude').val(theLong);
}