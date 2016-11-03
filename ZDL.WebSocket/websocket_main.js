var ws = new WebSocket("ws://localhost:50000");
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
            var input = $("input1");
            ws.send(input.value);
            output("send: " + input.value);
            input.value = "";
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