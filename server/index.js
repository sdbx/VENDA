let server = require('socket.io');
const io = server.listen(5353);

const userData = {};

io.on('connection', (socket) => {
  console.log('connected', socket.id);
  io.emit('newPlayer', { id: socket.id });
  socket.emit('info', {id: socket.id});
  socket.on('myData', (msg) => {
    userData[socket.id] = msg;
    userData[socket.id].lastPing = + new Date();
  });

  socket.on('animate', (msg) => {
    io.emit('animate', {id: socket.id, name: msg.name});
  });

  socket.on('hit', (msg) => {
    let data = msg;
    io.to(data.target).emit('hit', {dmg: data.dmg});
  });

  socket.on('disconnect', () => {
    console.log('disconnected', socket.id);
    io.emit('delPlayer', { id: socket.id});
    delete userData[socket.id];
  });
});

setInterval(() => {
  for (let [key, value] of Object.entries(userData)) {
    if (+ new Date() - value.lastPing > 2000) {
      delete userData[key];
    }
  }
  io.emit('userData', userData);
}, 1000/32.0);
