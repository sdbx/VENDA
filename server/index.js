let server = require('socket.io');
var fs = require('fs');
const io = server.listen(5353);

const isBench = false;
const userData = {};
const bench = {};
let pingId = 0;
let index = 0;

function shuffle(a) {
  for (let i = a.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [a[i], a[j]] = [a[j], a[i]];
  }
  return a;
}

const firstNames = ["걷는", "재빠른", "힘쎈", "나태한", "괴랄한", "요염한", "뚜따하는", "끼로끼로", "아싸", "인싸", "호에에"];
const secondNames = ["아재", "아저씨", "짱", "군", "아가씨", "님", "선배", "끼로", "교수", "선생", "박사", "할아버지", "할머니", "놈", "자식"];
const thirdNames = ["끼로", "작사", "루데브", "프구", "볼트", "알약", "벨붕", "해티", "탄라로", "나비", "도루", "티바이트", "쪼리핑", "플중", "인클", "ㄹ", "비양", "스라", "타자", "개돌이", "볕뉘"];
if (!(thirdNames.length >= secondNames.length && secondNames.length >= firstNames.length)) {
  throw new Error('1000 > 100 > 10')
}
const randIndice = shuffle([...Array(firstNames.length * secondNames.length * thirdNames.length).keys()]);
const generateName = () => {
  let ii = randIndice[index];
  let a = ii % firstNames.length;
  ii = Math.floor(ii/firstNames.length);
  let b = ii % secondNames.length;
  ii  =  Math.floor(ii/secondNames.length);
  let c = ii;
  index ++;
  if (index === firstNames.length * secondNames.length * thirdNames.length) {
    index = 0;
  }
  return firstNames[a] + thirdNames[c] + secondNames[b];
};

io.on('connection', (socket) => {
  console.log('connected', socket.id);
  io.emit('newPlayer', { id: socket.id });
  socket.emit('info', {id: socket.id, name: generateName(), map: fs.readFileSync('map.txt').toString()});
  socket.on('myData', (msg) => {
    userData[socket.id] = msg;
    userData[socket.id].lastPing = + new Date();
  });

  socket.on('animate', (msg) => {
    io.emit('animate', {id: socket.id, animeId: msg.animeId});
  });

  socket.on('hit', (msg) => {
    let data = msg;
    io.to(data.target).emit('hit', {dmg: data.dmg, id:socket.id});
  });

  socket.on('death', (msg) => {
    io.emit('death', {id: socket.id, by: msg.id});
  });
 
  socket.on('disconnect', () => {
    console.log('disconnected', socket.id);
    io.emit('delPlayer', { id: socket.id});
    delete userData[socket.id];
  });

  socket.on('pong', (msg) => {
    bench[msg.id].pongs[socket.id] = + new Date();
  });
});

setInterval(() => {
  for (let [key, value] of Object.entries(userData)) {
    if (+ new Date() - value.lastPing > 2000) {
      delete userData[key];
    }
  }
  io.emit('userData', userData);
  
  if (isBench) {
    const id = pingId.toString();
    bench[id] = {ping: + new Date(), clients: Object.keys(io.engine.clients), pongs: {}};
    io.emit('ping', {id: id});
    pingId ++;
  }
  
}, 1000/32.0);

if (isBench) {
  setInterval(() => {
    console.log('start');
    const data = [];
    for (let [key, value] of Object.entries(bench)) {
      if (+ new Date() - value.ping > 2000) {
        if (value.clients.length !== 0) {
          const id = key;
          const completed = Object.keys(value.pongs).length / value.clients.length;
          const numberOfClients = value.clients.length;
          const latencies = Object.values(value.pongs).map(x => x - value.ping );
          const minLat = Math.min(...latencies);
          const maxLat = Math.max(...latencies);
          const meanLat = latencies.reduce((a,b) => a + b, 0) / latencies.length;
          data.push({id: id, time: value.ping, completed: completed, numberOfClients: numberOfClients, minLat: minLat, maxLat: maxLat, meanLat: meanLat});
        } 
      }
    }
    var fs = require('fs');
    fs.writeFileSync('bench.json', JSON.stringify(data), (err) => {if (err) console.error(err)});
    console.log('wrote');
  }, 30000);
}