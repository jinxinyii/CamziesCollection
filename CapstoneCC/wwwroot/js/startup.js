const images = [
    "/images/homepage_picture/homepage_picture_1.jpg",
    "/images/homepage_picture/homepage_picture_2.jpg",
    "/images/homepage_picture/homepage_picture_3.jpg"
];

let currentIndex = 0;
let timer;

function changeImage(index) {
    console.log("Changing to image:", index); // DEBUGGER
    currentIndex = index;
    const productImage = document.getElementById("productImage");

    if (productImage) {
        productImage.src = images[currentIndex];
        console.log("Image source updated to:", productImage.src); // DEBUGGER
    }
    updateDots();
    resetTimer();
}

function updateDots() {
    const dots = document.querySelectorAll(".dots i");
    console.log("Updating dots for image:", currentIndex); // DEBUGGER
    dots.forEach((dot, index) => {
        dot.classList.toggle("fas", index === currentIndex);
        dot.classList.toggle("far", index !== currentIndex);
    });
}

function startSlideshow() {
    console.log("Starting slideshow"); // DEBUGGER
    timer = setInterval(() => {
        currentIndex = (currentIndex + 1) % images.length;
        changeImage(currentIndex);
        console.log("Automatic change to image:", currentIndex); // DEBUGGER
    }, 3000);
}

function resetTimer() {
    console.log("Resetting timer"); // DEBUGGER
    clearInterval(timer);
    startSlideshow();
}

window.addEventListener("load", function() {
    console.log("Page loaded, initializing slideshow"); // DEBUGGER
    changeImage(currentIndex);
    startSlideshow();
});