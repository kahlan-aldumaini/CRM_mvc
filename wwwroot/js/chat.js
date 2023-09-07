const chat = new signalR.HubConnectionBuilder().withUrl("/chat").build();
chat.on("ReceiveMessage", function (message, id, images) {
    console.log(message, id, images)
});

chat.on("UpdateChat", function (model) {
    if (model != null) {
        if (model.message) {
            console.log(model.message)
        } else {
            $("#sessionId").val(model.id)
            $('#customerName').html(model.name)
            if(model.closed){
                $('#closed').show()
                $('#submit-message-div').hide()
            }else{
                $('#closed').hide()
                $('#submit-message-div').show()
            }
            // current connection
            if (model.currentSessions) {
                let currentConversation = $('#currentConversations');
                currentConversation.html('')
                for (const item of model.currentSessions) {
                    let converse = `<div class="friend-drawer friend-drawer--onhover" style="${item.closed ? 'background-color:red' : ''}">
                            <div class="text text-white">
                                <h6>${item.phone}</h6>
                                <p class="text-muted">${item.message}</p>
                            </div>
                            <span class="time text-muted small">${item.time}</span>
                        </div>
                        <hr>`;
                    currentConversation.html(currentConversation.html() + converse)
                }
            }

            //messages
            if (model.messages) {
                let messages = $('#messages');
                for (const item of model.messages) {
                    let message = `<div id="${item.id}" class="no-gutters ${!item.fromUser ? 'd-flex justify-content-end' : ''}">
                                    <div class="col-md-3">
                                        <div class="chat-bubble">
                                            ${item.attachments.length > 0 ? `<img class="w-100 h-auto mb-1" src="${item.attachments[0].path}" style="width: 100px; height: 100px; border-radius: 10px">` : ''}
                                            ${item.message}
                                            <br>
                                                <div class="row">
                                                    <div class="col-8">
                                                        <span class="time text-muted small text-truncate">${item.dateTime}</span>
                                                    </div>
                                                    <div class="col-4">
                                                        <button class="btn p-0 " type="button" id="timelineWapper"
                                                                data-bs-toggle="dropdown" aria-haspopup="true"
                                                                aria-expanded="false">
                                                            <i class="bx bx-dots-vertical-rounded"></i>
                                                        </button>

                                                    </div>
                                                </div>
                                        </div>
                                    </div>
                                </div>`;
                    if (window.localStorage.getItem(item.id) != null && messages.find(`#${item.id}`).length === 0) {
                        messages.html(messages.html() + message)
                        continue;
                    } else if (window.localStorage.getItem(item.id) != null && messages.find(`#${item.id}`).length > 0) continue;
                    else if (window.localStorage.getItem(item.id) == null && messages.find(`#${item.id}`).length > 0) {
                        messages.find(`#${item.id}`).remove()
                    }
                    window.localStorage.setItem(item.id, JSON.stringify(item))
                    messages.html(messages.html() + message)
                }
            }
        }
    }
})

chat.start()

let sessionId = $('#sessionId')
let button = $('#submit-message')
let sendForm = $('#sendForm')
let message = $('#message')
let files = $('.files')
let previewImages = document.getElementById('preview-image')
let base64 = null;
let messages = $('#messages');

$(document).ready(function () {
    files.on('change', function (event) {
        for (const file of event.target.files) {
            const img = document.createElement('img')
            img.setAttribute('src', URL.createObjectURL(file));
            img.style.width = '50px';
            img.style.height = '50px';
            img.style.borderRadius = '10px';
            previewImages.classList.remove('d-none')
            previewImages.appendChild(img)
            let readers = new FileReader();
            readers.readAsDataURL(file);
            readers.onloadend = function () {
                base64 = readers.result.split('base64,')[1]
            }
        }
    });
})

sendForm.submit(function(e){
    e.preventDefault();
    chat.invoke("SendMessage", message.val(), sessionId.val(), base64)
        .catch(e => console.log(e))
        .then(v => {
            message.val('')
            base64 = null;
            previewImages.innerHTML = ''
            messages.animate({scrollTop: messages.prop("scrollHeight")}, {
                duration: 1000,
                easing: "swing"
            });
        })
})

function updateData(id) {
    chat.invoke("AskForUpdateChat", id)
        .catch(e => console.log(e));
    messages.animate({scrollTop: messages.prop("scrollHeight")}, {
        duration: 1000,
        easing: "swing"
    });
}
