let connection = new signalR.HubConnectionBuilder().withUrl("/receiveCodeHub").build();
connection.start().then(() => {
    connection.invoke("getconnectionid").then(id => {
        document.querySelector("#linkToTelegram").href = `https://t.me/SupedDuperPuperGroupbot?start=${id}`
    })
})
connection.on("ReceiveMessage", function (message) {
    let inputElement = document.querySelector("#number")
    inputElement.disabled = true
    inputElement.value = message;
});