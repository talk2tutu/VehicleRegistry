import { useState } from 'react'
import { VehiclesPage } from './components/VehiclesPage'
import { CategoriesPage } from './components/CategoriesPage'

type Tab = 'vehicles' | 'categories'

export default function App() {
  const [tab, setTab] = useState<Tab>('vehicles')

  return (
    <div className="app">
      <header className="app-header">
        <h1>Vehicle Registry</h1>
        <nav aria-label="Main">
          <button
            type="button"
            className={tab === 'vehicles' ? 'tab active' : 'tab'}
            aria-current={tab === 'vehicles' ? 'page' : undefined}
            onClick={() => setTab('vehicles')}
          >
            Vehicles
          </button>
          <button
            type="button"
            className={tab === 'categories' ? 'tab active' : 'tab'}
            aria-current={tab === 'categories' ? 'page' : undefined}
            onClick={() => setTab('categories')}
          >
            Categories
          </button>
        </nav>
      </header>

      <main>
        {tab === 'vehicles' ? (
          <VehiclesPage />
        ) : (
          <CategoriesPage />
        )}
      </main>
    </div>
  )
}