export type User = { id:string; storeId?:string; username:string; displayName:string; role:string; permissionsCsv:string; theme:string; language:string }

const TOKEN_KEY='cellerp_token'
export const getToken=()=>localStorage.getItem(TOKEN_KEY)
export const setToken=(token:string|null)=> token ? localStorage.setItem(TOKEN_KEY,token) : localStorage.removeItem(TOKEN_KEY)

export async function api<T=any>(path:string, options:RequestInit={}):Promise<T>{
  const token=getToken()
  const headers=new Headers(options.headers)
  if(options.body && !headers.has('Content-Type')) headers.set('Content-Type','application/json')
  if(token) headers.set('Authorization',`Bearer ${token}`)
  const res=await fetch(path,{...options,headers})
  if(res.status===401){ setToken(null); if(location.pathname!='/login') location.href='/login'; throw new Error('Sesión vencida') }
  if(!res.ok){ const body=await res.text(); throw new Error(body || `HTTP ${res.status}`) }
  if(res.status===204) return undefined as T
  return res.json()
}

export const money=(value:number|undefined,currency='COP')=>new Intl.NumberFormat('es-CO',{style:'currency',currency,maximumFractionDigits:0}).format(value??0)
export const dateTime=(value:string|undefined)=>value?new Intl.DateTimeFormat('es-CO',{dateStyle:'medium',timeStyle:'short'}).format(new Date(value)):'—'
