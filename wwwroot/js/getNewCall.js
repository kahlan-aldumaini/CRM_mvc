var connection = new signalR.HubConnectionBuilder().withUrl("/hasCall").build();
var state = '';
connection.on("ReceiveCall", function (hasCall) {
    if (hasCall.hasCall) {
        document.getElementById("newCallSideBar").classList.add("show");
        document.getElementById('newCallPhoneNumber').innerText = hasCall.phone;
        document.getElementById('newCallName').innerText = hasCall.name;
        document.getElementById('newCallNextUrl').href = hasCall.nextUrl;
    } else {
        document.getElementById("newCallSideBar").classList.remove("show");
    }
    state = hasCall.state;
});

connection.start()

setInterval(() => connection.invoke("HasCall", state), 3000);
