


var ws = new WebSocket("ws://127.0.0.1:50000");

        ws.onopen = function() {
            output("onopen");
        };
        ws.onmessage = function(e) {
            output("onmessage: " + e.data);
        };
        ws.onclose = function() {
            output("onclose");
        };
        ws.onerror = function() {
            output("onerror");
        };
    $('#send').on('click',function(){
        var input = $("#input1");
      
            ws.send(input.val());
            output("send: " + input.val());
            input.val("");
            input.focus();
    });
    $('#close').on('click',function(){
            ws.close();
    });
    function output(str) {
        var log = document.getElementById("log");
        var escaped = str.replace(/&/, "&amp;").replace(/</, "&lt;")
                .replace(/>/, "&gt;").replace(/"/, "&quot;"); // "
        log.innerHTML = escaped + "<br>" + log.innerHTML;
    }