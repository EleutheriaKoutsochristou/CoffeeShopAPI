const menuContainer = document.getElementById('menuContainer')
const refreshBtn = document.getElementById('refreshBtn')
const buyCoffeeBtn = document.getElementById('buyCoffeeBtn')
const orderStatus = document.getElementById('orderStatus')

const API_URL = 'http://localhost:5001/api'

async function loadMenu() {
  try {
    orderStatus.textContent = 'Loading menu...'
    const response = await fetch(`${API_URL}/coffeemenu`)

    if (!response.ok) {
      throw new Error('Unable to load coffee menu')
    }

    const menu = await response.json()

    menuContainer.innerHTML = ''

    menu.forEach((item) => {
      const card = document.createElement('article')
      card.className = 'menu-item'
      card.innerHTML = `
        <h3>${item.name}</h3>
        <p>${item.description}</p>
        <span class="price">€${Number(item.price).toFixed(2)}</span>
      `
      menuContainer.appendChild(card)
    })

    orderStatus.textContent = 'Menu loaded successfully.'
  } catch (error) {
    orderStatus.textContent =
      'Could not load menu. Make sure the API is running.'
    console.error(error)
  }
}

async function buyCoffee() {
  try {
    orderStatus.textContent = 'Processing payment...'

    const response = await fetch(`${API_URL}/order/buy-coffee`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
    })

    const text = await response.text()
    orderStatus.textContent = text || 'Order completed.'
  } catch (error) {
    orderStatus.textContent = 'Error while placing order.'
    console.error(error)
  }
}

refreshBtn.addEventListener('click', loadMenu)
buyCoffeeBtn.addEventListener('click', buyCoffee)

loadMenu()
