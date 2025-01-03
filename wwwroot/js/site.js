document.getElementById('reservationForm').addEventListener('submit', function(e) {
    e.preventDefault();
    const form = e.target;

    fetch(form.action, {
        method: 'POST',
        body: new FormData(form),
        headers: {
            'Accept': 'application/json'
        }
    })
    .then(response => response.json())
    .then(data => {
        if (data.redirectTo) {
            showPopup(data.message, data.redirectTo);
        } else {
            showPopup(data.message);
        }
    })
    .catch(error => {
        showPopup('There was an error submitting your reservation.');
    });
});

function showPopup(message, redirectTo = null) {
    const popup = document.getElementById('popup');
    const popupMessage = document.getElementById('popupMessage');
    const popupButton = document.getElementById('popupButton');

    popupMessage.textContent = message;
    popup.classList.remove('hidden');

    popupButton.addEventListener('click', function() {
        popup.classList.add('hidden');
        if (redirectTo) {
            window.location.href = redirectTo;
        }
    });
}
