import { createContext, useContext, useMemo, useState, type ReactNode } from 'react'
import es from './locales/es.json'; import en from './locales/en.json'; import pt from './locales/pt.json'
type Lang='es'|'en'|'pt'; const packs={es,en,pt};
const C=createContext({lang:'es' as Lang,setLang:(_:Lang)=>{},t:(k:string)=>k})
export function I18nProvider({children}:{children:ReactNode}){ const [lang,setLangState]=useState<Lang>((localStorage.getItem('cellerp_lang') as Lang)||'es'); const setLang=(x:Lang)=>{localStorage.setItem('cellerp_lang',x);setLangState(x)}; const value=useMemo(()=>({lang,setLang,t:(k:string)=>(packs[lang] as Record<string,string>)[k]??k}),[lang]);return <C.Provider value={value}>{children}</C.Provider> }
export const useI18n=()=>useContext(C)
