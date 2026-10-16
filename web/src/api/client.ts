import type {
  Category,
  CategoryRequest,
  CreateVehicleRequest,
  Manufacturer,
  SortDirection,
  Vehicle,
  VehicleSortField,
} from './types'

/** Error thrown for any non-success response; `errors` holds the server's validation messages. */
export class ApiError extends Error {
  readonly errors: string[]

  constructor(message: string, errors: string[] = []) {
    super(message)
    this.errors = errors
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(path, {
    ...init,
    headers: { 'Content-Type': 'application/json', ...init?.headers },
  })

  if (!response.ok) {
    let errors: string[] = []
    try {
      const body = await response.json()
      errors = body?.errors?.validation ?? []
    } catch {
      // response had no JSON body
    }
    throw new ApiError(
      errors.length > 0 ? 'Please fix the problems below.' : `Request failed (${response.status}).`,
      errors,
    )
  }

  if (response.status === 204) return undefined as T
  return (await response.json()) as T
}

export const api = {
  getManufacturers: () => request<Manufacturer[]>('/api/manufacturers'),

  getVehicles: (sortBy: VehicleSortField, sortDir: SortDirection) =>
    request<Vehicle[]>(`/api/vehicles?sortBy=${sortBy}&sortDir=${sortDir}`),

  createVehicle: (body: CreateVehicleRequest) =>
    request<Vehicle>('/api/vehicles', { method: 'POST', body: JSON.stringify(body) }),

  getCategories: () => request<Category[]>('/api/categories'),

  getIcons: () => request<string[]>('/api/categories/icons'),

  createCategory: (body: CategoryRequest) =>
    request<Category>('/api/categories', { method: 'POST', body: JSON.stringify(body) }),

  updateCategory: (id: number, body: CategoryRequest) =>
    request<Category>(`/api/categories/${id}`, { method: 'PUT', body: JSON.stringify(body) }),

  deleteCategory: (id: number) =>
    request<void>(`/api/categories/${id}`, { method: 'DELETE' }),
}