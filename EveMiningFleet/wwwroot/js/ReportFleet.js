

/* petit bout de vania JS pour afficher une popup
   
   fonctionnement :
    - mettre un id "popup1" et une class "popup" au popup
    - mettre une class "popup1-button" au(x) bouton(s)
*/

// setup des Events
for(i of document.getElementsByClassName('popup')) {  // boucler dans tout les elements de class "popup"
    buttons = document.getElementsByClassName(i.id + '-button')  // recupérer le bouton associé au popup

    for(button of buttons) {

        button.addEventListener('click', function(e) {  // ajout de l'Event

            // boucler dans les class du bouton pour retrouver une class qui est relié à une popup
            for(j of e.target.classList) {

                // checher si le class corespond avec le bon format
                if(!j.includes('-button'))  continue
                else if(!document.getElementById(j.replace('-button', '')))  continue
                
                popup = document.getElementById(j.replace('-button', ''))  // recupérer le popup associé au bouton
                body = document.getElementsByTagName('body')[0]
                
                // assigner la class "popup--open" si elle n'y est pas et la retirer si elle y est deja
                // tent qu'a y être bloquer aussi le scroll
                if(popup.classList.contains('popup--open')) {
                    popup.className = popup.className.replace(' popup--open', '')
                    body.style.overflowY = 'auto'
                }
                else {
                    popup.className += ' popup--open'
                    body.style.overflowY = 'clip'
                }
            }

        })

    }
}


/* petit bout de vania JS pour copier le lien dans le clipboard
   
   fonctionnement :
    - mettre la class "link__button" au bouton
*/

// setup des Events
for(i of document.getElementsByClassName('link__button')) {  // boucler dans tout les elements de class "link__button"

    // re set la value de début à chaque changment
    i.addEventListener('click', function(e) {
         //annmation
        e.target.animate([
                // keyframes
                { transform: 'rotateY(0deg)' },
                { transform: 'rotateY(180deg)' }
            ], {
            duration: 200
        });

        for(j of e.target.parentElement.children) {
            
            if(!j.classList.contains('link__input-box'))  continue

            for(k of j.children) {
            
                if(!k.classList.contains('link__input-box__input'))  continue

                value = k.value
                break
            }
            break
        }
        
        // copying to clipboard
        navigator.clipboard.writeText(value).then(function() {
            console.info('link copyed to your clipboard')
        }, function() {
            console.error('we can\'t copy the link to your clipboard')
        });
    })
}