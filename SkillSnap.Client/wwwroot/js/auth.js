window.skillSnapAuth = {
    setToken: function (key, value) {
        localStorage.setItem(key, value);
    },
    getToken: function (key) {
        return localStorage.getItem(key);
    },
    removeToken: function (key) {
        localStorage.removeItem(key);
    }
};
