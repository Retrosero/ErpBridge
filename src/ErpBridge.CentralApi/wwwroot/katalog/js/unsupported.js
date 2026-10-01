// Classic script loaded with <script nomodule>: only browsers without ES modules run it, and they
// cannot run the catalogue either. ES5 on purpose.
(function () {
    var boot = document.getElementById('boot');
    if (!boot) return;
    boot.textContent = 'Tarayıcınız bu sayfayı desteklemiyor. Kataloğu açmak için tarayıcınızı güncelleyin ya da güncel Chrome veya Safari kullanın.';
    boot.className = 'boot boot--message';
})();
