import { useState } from 'react'

function App() {
  const [count, setCount] = useState(0)

  return (
      <>
        <p>{count}</p>
        <button onClick={() => {
            setCount(count+1)
        }}>click me</button>
      </>
  )
}

export default App
