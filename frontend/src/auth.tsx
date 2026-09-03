import { createContext,useContext,useEffect,useState,type ReactNode } from 'react'
import { api,getToken,setToken,type User } from './api'
type Auth={user:User|null; store:any; loading:boolean; login:(u:string,p:string)=>Promise<void>; logout:()=>void;refresh:()=>Promise<void>}
const C=createContext<Auth>(null as any)
export function AuthProvider({children}:{children:ReactNode}){const[user,setUser]=useState<User|null>(null);const[store,setStore]=useState<any>(null);const[loading,setLoading]=useState(true);const refresh=async()=>{if(!getToken()){setLoading(false);return}try{const x=await api<any>('/api/auth/me');setUser(x.user);setStore(x.store)}catch{setToken(null);setUser(null)}finally{setLoading(false)}};useEffect(()=>{refresh()},[]);const login=async(u:string,p:string)=>{const x=await api<any>('/api/auth/login',{method:'POST',body:JSON.stringify({username:u,password:p})});setToken(x.token);setUser(x.user);await refresh()};const logout=()=>{setToken(null);setUser(null);location.href='/login'};return <C.Provider value={{user,store,loading,login,logout,refresh}}>{children}</C.Provider>}
export const useAuth=()=>useContext(C)
