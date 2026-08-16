const menuContainer = document.getElementById('menuContainer')
const refreshBtn = document.getElementById('refreshBtn')
const buyCoffeeBtn = document.getElementById('buyCoffeeBtn')
const orderStatus = document.getElementById('orderStatus')

const API_URL = 'http://localhost:5001/api'
let selectedCoffee = null

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
        <button class="select-btn" data-name="${item.name}">Select</button>
      `

      const selectButton = card.querySelector('.select-btn')
      selectButton.addEventListener('click', () => {
        selectedCoffee = item.name
        document.querySelectorAll('.select-btn').forEach((btn) => {
          btn.classList.remove('selected')
        })
        selectButton.classList.add('selected')
        orderStatus.textContent = `${item.name} selected. Ready to order.`
      })

      menuContainer.appendChild(card)
    })

    if (!selectedCoffee && menu.length > 0) {
      selectedCoffee = menu[0].name
      orderStatus.textContent = `${selectedCoffee} selected by default. Ready to order.`
    }
  } catch (error) {
    orderStatus.textContent =
      'Could not load menu. Make sure the API is running.'
    console.error(error)
  }
}

async function buyCoffee() {
  try {
    if (!selectedCoffee) {
      orderStatus.textContent = 'Please select a coffee first.'
      return
    }

    orderStatus.textContent = 'Processing payment...'

    const response = await fetch(`${API_URL}/order/buy-coffee`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ coffeeName: selectedCoffee }),
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
