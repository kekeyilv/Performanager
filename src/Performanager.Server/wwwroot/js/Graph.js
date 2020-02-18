function Draw(canvas, data,max,unit) {
    canvas.width = canvas.clientWidth;
    var context = canvas.getContext("2d");
    context.strokeStyle = context.fillStyle = "royalblue";
    var maxn = 0;
    context.clearRect(0, 0, canvas.clientWidth, canvas.clientHeight);
    maxn = Math.max.apply(null,data);
    if (max !== null) {
        if (maxn < max) {
            maxn = max;
        }
    }
    maxn = Math.ceil(maxn);
    var ratioh = (300 - 40) / (maxn + 0.000000001);
    var ratiow = canvas.clientWidth / (data.length + 0.000000001);
    console.log(maxn + ":" +ratioh);
    var lastx = -ratiow;
    context.moveTo(0, canvas.clientHeight);
    context.beginPath();
    data.forEach(function (item) {
        var y = canvas.clientHeight-ratioh * item - 10;
        var x = lastx + ratiow;
        context.lineTo(x + 10, y);
        var str = item.toString();
        if (unit !== undefined) {
            str += " "+unit;
        }
        context.strokeText(str, x, y - 15);
        context.moveTo(x+10, y);
        lastx = x;
    });
    context.stroke();
    context.closePath();
    lastx = -ratiow;
    data.forEach(function (item) {    
        var y = canvas.clientHeight - ratioh * item - 10;
        var x = lastx + ratiow;
        context.beginPath();
        context.arc(x + 10, y, 3, 0, 2 * Math.PI);
        context.fill();
        context.closePath();
        lastx = x;
    });
}