const
    io = require("socket.io-client"),
    ioClient = io.connect("http://localhost:5353");
ioClient.emit('myData', {x: 300, y: 400, hp: 30})
ioClient.on("userData", (msg) => console.info(msg));