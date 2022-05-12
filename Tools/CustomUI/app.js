var http = require('http');
var fs = require('fs');
const portNo = 8080;

http.createServer( function (request, response) {  
   var pathname = request.url;
   if('/' === pathname) {
      pathname = '/index.html';
   }
   console.log("Request for " + pathname + " received.");
   fs.readFile(pathname.substr(1), function (err, data) {
      if (err) {
         console.log(err);
         response.writeHead(404, {'Content-Type': 'text/html' });
      } else {	
         response.writeHead(200, {'Content-Type': 'text/html', 'access-control-allow-origin': '*' });	
         response.write(data.toString());		
      }
      response.end();
   });   
}).listen(portNo);

console.log(`Server running at http://127.0.0.1:${portNo}/`);
