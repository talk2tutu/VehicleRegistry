const icons: Record<string, string> = {
  bike: '🚲',
  car: '🚗',
  van: '🚐',
  truck: '🚚',
  bus: '🚌',
  tractor: '🚜',
}

interface Props {
  iconKey: string
  label: string
}

export function CategoryIcon({ iconKey, label }: Props) {
  return (
    <span className="category-icon" role="img" aria-label={label} title={label}>
      {icons[iconKey] ?? '❔'}
    </span>
  )
}