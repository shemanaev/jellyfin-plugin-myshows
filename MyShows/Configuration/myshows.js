define(['loading', 'globalize', 'dom', 'dashboardcss', 'emby-input', 'emby-button', 'emby-select', 'emby-checkbox'], function (loading, globalize, dom) {
    'use strict';

    const pluginId = 'ef35f6b1-7fe6-44ca-a215-232089fb9bc7'
    let configCache = []

    function onViewShow() {
        const page = this
        page.querySelector('#create-account').innerHTML = globalize.translate('MessageCreateAccountAt', 'MyShows')
        loadConfig(page)
    }

    function loadConfig(page) {
        loading.show()

        return ApiClient.getPluginConfiguration(pluginId).then((config) => {
            configCache = config
            const userId = ApiClient.getCurrentUserId()
            const user = config.Users.find(e => e.Id == userId && e.AccessToken != null && e.AccessToken != '')

            if (user) {
                page.querySelector('#ScrobbleAt').value = user.ScrobbleAt
                page.querySelector('#linked-account').innerText = user.Name
                page.querySelector('#config-container').style.display = 'block'
                page.querySelector('#login-container').style.display = 'none'
            } else {
                page.querySelector('#config-container').style.display = 'none'
                page.querySelector('#login-container').style.display = 'block'
            }

            loading.hide()
        })
    }

    function saveConfig(page) {
        const userId = ApiClient.getCurrentUserId()
        const user = configCache.Users.find(e => e.Id == userId)
        user.ScrobbleAt = page.querySelector("#config-container #ScrobbleAt").value
        return ApiClient.updatePluginConfiguration(pluginId, configCache).then(Dashboard.processPluginConfigurationUpdateResult)
    }

    function onLoginClick(e) {
        const page = dom.parentWithClass(this, 'page')
        loading.show()

        const userId = ApiClient.getCurrentUserId()
        const apiParams = {
            login: page.querySelector('#login').value,
            password: page.querySelector('#password').value
        }
        const request = {
            type: 'POST',
            url: ApiClient.getUrl('/MyShows/v2/login/' + userId),
            headers: { accept: "application/json" },
            data: apiParams
        }
        ApiClient.fetch(request).then((result) => {
            if (!result.success) {
                Dashboard.alert({
                    message: 'An error occurred when trying to authorize device: ' + result.statusText
                })
            } else {
                loadConfig(page).then(() => saveConfig(page))
            }
        }).finally(() => loading.hide())

        return false
    }

    function onUnlinkClick() {
        const page = dom.parentWithClass(this, 'page')
        const userId = ApiClient.getCurrentUserId()
        configCache.Users = configCache.Users.filter(e => e.Id != userId)
        saveConfig(page).then(() => loadConfig(page))
    }

    function onFormSubmit() {
        const page = dom.parentWithClass(this, 'page')
        saveConfig(page)
        return false
    }

    return function (view, params) {
        view.addEventListener('viewshow', onViewShow)
        view.querySelector('#MyShowsConfigForm').addEventListener('submit', onFormSubmit)
        view.querySelector('#login-button').addEventListener('click', onLoginClick)
        view.querySelector('#unlink-button').addEventListener('click', onUnlinkClick)
    }
});
