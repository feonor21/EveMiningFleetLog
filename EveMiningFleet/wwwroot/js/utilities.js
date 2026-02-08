
function displayedPrice(price) {
    // function stringify(nbr) {
    //     nbr = String(nbr).split('.')
    //     v0 = ''

    //     for(var i=nbr[0].length-1; i>=0; i--) {
    //         v0 = (i%3 == 0 && i != nbr[0].length-1 ? nbr[0][i]+' ' : nbr[0][i])  +  v0
    //     }

    //     // is il n'y a pas de nombre après la virgule
    //     if(nbr[1] == undefined) {
    //         return v0
    //     }
    //     // is il y a un nombre après la virgule
    //     else {
    //         return v0 + '.' + nbr[1]
    //     }
    // }

    console.log(price);
    if(price >= 1000000000) {
        return String(Math.round(price / 100000000) / 10) + 'B'
    }
    if(price >= 1000000) {
        return String(Math.round(price / 100000) / 10) + 'M'
    }
    if(price >= 10000) {
        return String(Math.round(price / 100) / 10) + 'k'
    }
    else {
        return String(Math.round(price * 10) / 10)
    }
}
