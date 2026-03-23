import { useState, useEffect } from 'react'
import { Eye, EyeOff, Plus, Pencil, Trash2 } from 'lucide-react'
import { useApiProvider } from '../services/apiProvider'
import { ExchangeName } from '../api'
import type {
  UserDto,
  ExchangeConnectionResponse,
  ExchangeNameExchangeConnectionResponseDictionaryApiResponseData,
} from '../api'
import '../styles/settings.css'

type Tab = 'profile' | 'apikeys'
type ExchangeKey = 'Binance' | 'Okx' | 'Bybit'

const EXCHANGES: ExchangeKey[] = ['Binance', 'Okx', 'Bybit']

function extractApiError(err: unknown): string | null {
  if (err && typeof err === 'object' && 'response' in err) {
    const res = (err as { response?: { data?: { message?: string } } }).response
    return res?.data?.message ?? null
  }
  return null
}

function formatDate(dateStr?: string | null): string {
  if (!dateStr) return 'Never'
  return new Date(dateStr).toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' })
}

// ── ProfileTab ───────────────────────────────────────────────

type PwForm = { currentPassword: string; newPassword: string; confirmPassword: string }
type ShowPw = Record<'current' | 'new' | 'confirm', boolean>

interface ProfileTabProps {
  user: UserDto | null
  userLoading: boolean
  pwForm: PwForm
  setPwForm: React.Dispatch<React.SetStateAction<PwForm>>
  pwLoading: boolean
  pwFeedback: { type: 'success' | 'error'; message: string } | null
  showPw: ShowPw
  setShowPw: React.Dispatch<React.SetStateAction<ShowPw>>
  onSubmit: (e: React.FormEvent) => void
}

function ProfileTab({ user, userLoading, pwForm, setPwForm, pwLoading, pwFeedback, showPw, setShowPw, onSubmit }: ProfileTabProps) {
  const togglePw = (field: keyof ShowPw) =>
    setShowPw(prev => ({ ...prev, [field]: !prev[field] }))

  return (
    <>
      {/* Account Information */}
      <div className="settings-section">
        <div className="settings-section-header">
          <span className="settings-section-title">Account Information</span>
        </div>
        <div className="settings-section-body">
          {userLoading ? (
            <p style={{ color: 'var(--color-text-secondary)', fontSize: 13 }}>Loading...</p>
          ) : (
            <>
              <div className="settings-field">
                <span className="settings-field-label">Username</span>
                <span className="settings-field-value">{user?.username ?? '—'}</span>
              </div>
              <div className="settings-field">
                <span className="settings-field-label">Roles</span>
                {user?.roles && user.roles.length > 0 ? (
                  <div className="settings-roles-list">
                    {user.roles.map(role => (
                      <span key={role} className="settings-role-badge">{role}</span>
                    ))}
                  </div>
                ) : (
                  <span className="settings-field-value">—</span>
                )}
              </div>
            </>
          )}
        </div>
      </div>

      {/* Change Password */}
      <div className="settings-section">
        <div className="settings-section-header">
          <span className="settings-section-title">Change Password</span>
        </div>
        <div className="settings-section-body">
          {pwFeedback && (
            <div className={`settings-feedback ${pwFeedback.type}`}>{pwFeedback.message}</div>
          )}
          <form onSubmit={onSubmit}>
            <div className="settings-form-grid">
              {(
                [
                  { key: 'current', label: 'Current Password', placeholder: 'Enter current password' },
                  { key: 'new', label: 'New Password', placeholder: 'Enter new password' },
                  { key: 'confirm', label: 'Confirm New Password', placeholder: 'Confirm new password' },
                ] as const
              ).map(({ key, label, placeholder }) => (
                <div key={key} className="control-group">
                  <label>{label}</label>
                  <div className="settings-key-wrap">
                    <input
                      type={showPw[key] ? 'text' : 'password'}
                      placeholder={placeholder}
                      value={pwForm[`${key}Password` as keyof PwForm]}
                      onChange={e =>
                        setPwForm(prev => ({ ...prev, [`${key}Password`]: e.target.value }))
                      }
                      required
                    />
                    <button
                      type="button"
                      className="settings-key-eye"
                      onClick={() => togglePw(key)}
                      tabIndex={-1}
                    >
                      {showPw[key] ? <EyeOff /> : <Eye />}
                    </button>
                  </div>
                </div>
              ))}
            </div>
            <div className="form-actions">
              <button type="submit" className="btn btn-primary" disabled={pwLoading}>
                {pwLoading ? 'Saving...' : 'Update Password'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </>
  )
}

// ── ApiKeysTab ───────────────────────────────────────────────

interface ApiKeysTabProps {
  exchanges: ExchangeNameExchangeConnectionResponseDictionaryApiResponseData | null
  exchangesLoading: boolean
  activeForm: ExchangeKey | null
  formMode: 'add' | 'edit'
  formData: { apiKey: string; secretKey: string; isTestnet: boolean }
  setFormData: React.Dispatch<React.SetStateAction<{ apiKey: string; secretKey: string; isTestnet: boolean }>>
  formLoading: boolean
  formFeedback: string | null
  showSecret: boolean
  setShowSecret: (v: boolean) => void
  deleteLoading: ExchangeKey | null
  onOpenAdd: (exchange: ExchangeKey) => void
  onOpenEdit: (exchange: ExchangeKey, existing: ExchangeConnectionResponse) => void
  onClose: () => void
  onSave: (exchange: ExchangeKey) => void
  onDelete: (exchange: ExchangeKey) => void
}

function ApiKeysTab({
  exchanges, exchangesLoading, activeForm, formMode, formData, setFormData,
  formLoading, formFeedback, showSecret, setShowSecret,
  deleteLoading, onOpenAdd, onOpenEdit, onClose, onSave, onDelete,
}: ApiKeysTabProps) {
  return (
    <div className="settings-section">
      <div className="settings-section-header">
        <span className="settings-section-title">Exchange Connections</span>
      </div>

      {exchangesLoading ? (
        <div style={{ padding: '24px', color: 'var(--color-text-secondary)', fontSize: 13 }}>
          Loading...
        </div>
      ) : (
        EXCHANGES.map(exchange => {
          const conn = exchanges?.[exchange as keyof typeof exchanges] as ExchangeConnectionResponse | undefined
          const isConnected = !!conn
          const formOpen = activeForm === exchange

          return (
            <div key={exchange}>
              <div className="settings-exchange-row">
                <div className="settings-exchange-icon">
                  {exchange[0]}
                </div>
                <div className="settings-exchange-info">
                  <div className="settings-exchange-name">{exchange}</div>
                  {isConnected ? (
                    <div className="settings-exchange-meta">
                      <span className="settings-exchange-connected">Connected</span>
                      {conn.isTestnet && ' · Testnet'}
                      {conn.createdAt && ` · Added ${formatDate(conn.createdAt)}`}
                      {conn.lastUsedAt && ` · Last used ${formatDate(conn.lastUsedAt)}`}
                    </div>
                  ) : (
                    <div className="settings-exchange-meta">Not configured</div>
                  )}
                </div>
                <div className="settings-exchange-actions">
                  {isConnected ? (
                    <>
                      <button
                        className="btn btn-secondary btn-sm"
                        onClick={() => formOpen ? onClose() : onOpenEdit(exchange, conn)}
                      >
                        <Pencil size={12} /> Edit
                      </button>
                      <button
                        className="btn btn-danger btn-sm"
                        onClick={() => onDelete(exchange)}
                        disabled={deleteLoading === exchange}
                      >
                        <Trash2 size={12} />
                        {deleteLoading === exchange ? 'Removing...' : 'Remove'}
                      </button>
                    </>
                  ) : (
                    <button
                      className="btn btn-primary btn-sm"
                      onClick={() => formOpen ? onClose() : onOpenAdd(exchange)}
                    >
                      <Plus size={12} /> Connect
                    </button>
                  )}
                </div>
              </div>

              {formOpen && (
                <div className="settings-exchange-form">
                  <div className="settings-exchange-form-grid">
                    <div className="control-group">
                      <label>API Key</label>
                      <input
                        type="text"
                        placeholder={formMode === 'edit' ? 'Enter new API key to replace' : 'Paste your API key'}
                        value={formData.apiKey}
                        onChange={e => setFormData(prev => ({ ...prev, apiKey: e.target.value }))}
                        autoComplete="off"
                      />
                    </div>
                    <div className="control-group">
                      <label>Secret Key</label>
                      <div className="settings-key-wrap">
                        <input
                          type={showSecret ? 'text' : 'password'}
                          placeholder={formMode === 'edit' ? 'Enter new secret to replace' : 'Paste your secret key'}
                          value={formData.secretKey}
                          onChange={e => setFormData(prev => ({ ...prev, secretKey: e.target.value }))}
                          autoComplete="new-password"
                        />
                        <button
                          type="button"
                          className="settings-key-eye"
                          onClick={() => setShowSecret(!showSecret)}
                          tabIndex={-1}
                        >
                          {showSecret ? <EyeOff /> : <Eye />}
                        </button>
                      </div>
                    </div>
                  </div>

                  <div className="settings-testnet-row">
                    <input
                      id={`testnet-${exchange}`}
                      type="checkbox"
                      className="settings-testnet-cb"
                      checked={formData.isTestnet}
                      onChange={e => setFormData(prev => ({ ...prev, isTestnet: e.target.checked }))}
                    />
                    <label htmlFor={`testnet-${exchange}`} className="settings-testnet-label">
                      Testnet mode
                    </label>
                  </div>

                  {formFeedback && (
                    <div className="settings-feedback error">{formFeedback}</div>
                  )}

                  <div className="form-actions">
                    <button
                      className="btn btn-primary btn-sm"
                      onClick={() => onSave(exchange)}
                      disabled={formLoading || !formData.apiKey || !formData.secretKey}
                    >
                      {formLoading ? 'Saving...' : formMode === 'add' ? 'Connect' : 'Save Changes'}
                    </button>
                    <button className="btn btn-ghost btn-sm" onClick={onClose}>
                      Cancel
                    </button>
                  </div>
                </div>
              )}
            </div>
          )
        })
      )}
    </div>
  )
}

// ── Settings page ────────────────────────────────────────────

export function Settings() {
  const apiProvider = useApiProvider()

  const [activeTab, setActiveTab] = useState<Tab>('profile')

  // Profile state
  const [user, setUser] = useState<UserDto | null>(null)
  const [userLoading, setUserLoading] = useState(false)

  // Password state
  const [pwForm, setPwForm] = useState<PwForm>({ currentPassword: '', newPassword: '', confirmPassword: '' })
  const [pwLoading, setPwLoading] = useState(false)
  const [pwFeedback, setPwFeedback] = useState<{ type: 'success' | 'error'; message: string } | null>(null)
  const [showPw, setShowPw] = useState<ShowPw>({ current: false, new: false, confirm: false })

  // Exchanges state
  const [exchanges, setExchanges] = useState<ExchangeNameExchangeConnectionResponseDictionaryApiResponseData | null>(null)
  const [exchangesLoading, setExchangesLoading] = useState(false)

  // Exchange form state
  const [activeForm, setActiveForm] = useState<ExchangeKey | null>(null)
  const [formMode, setFormMode] = useState<'add' | 'edit'>('add')
  const [formData, setFormData] = useState({ apiKey: '', secretKey: '', isTestnet: false })
  const [formLoading, setFormLoading] = useState(false)
  const [formFeedback, setFormFeedback] = useState<string | null>(null)
  const [showSecret, setShowSecret] = useState(false)
  const [deleteLoading, setDeleteLoading] = useState<ExchangeKey | null>(null)

  useEffect(() => {
    fetchUser()
    fetchExchanges()
  }, [])

  async function fetchUser() {
    setUserLoading(true)
    try {
      const res = await apiProvider.getUserApi().apiUserMeGet()
      setUser((res.data as unknown as { data: UserDto }).data ?? null)
    } finally {
      setUserLoading(false)
    }
  }

  async function fetchExchanges() {
    setExchangesLoading(true)
    try {
      const res = await apiProvider.getUserApi().apiUserExchangesGet()
      setExchanges(
        (res.data as unknown as { data: ExchangeNameExchangeConnectionResponseDictionaryApiResponseData }).data ?? null
      )
    } finally {
      setExchangesLoading(false)
    }
  }

  async function handlePasswordSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (pwForm.newPassword !== pwForm.confirmPassword) {
      setPwFeedback({ type: 'error', message: 'New passwords do not match.' })
      return
    }
    setPwLoading(true)
    setPwFeedback(null)
    try {
      await apiProvider.getUserApi().apiUserPasswordPut({
        currentPassword: pwForm.currentPassword,
        newPassword: pwForm.newPassword,
        confirmPassword: pwForm.confirmPassword,
      })
      setPwFeedback({ type: 'success', message: 'Password updated successfully.' })
      setPwForm({ currentPassword: '', newPassword: '', confirmPassword: '' })
    } catch (err) {
      const msg = extractApiError(err) ?? 'Failed to update password.'
      setPwFeedback({ type: 'error', message: msg })
    } finally {
      setPwLoading(false)
    }
  }

  function openAddForm(exchange: ExchangeKey) {
    setFormData({ apiKey: '', secretKey: '', isTestnet: false })
    setFormMode('add')
    setActiveForm(exchange)
    setFormFeedback(null)
    setShowSecret(false)
  }

  function openEditForm(exchange: ExchangeKey, existing: ExchangeConnectionResponse) {
    setFormData({ apiKey: '', secretKey: '', isTestnet: existing.isTestnet ?? false })
    setFormMode('edit')
    setActiveForm(exchange)
    setFormFeedback(null)
    setShowSecret(false)
  }

  function closeForm() {
    setActiveForm(null)
    setFormFeedback(null)
  }

  async function handleSave(exchange: ExchangeKey) {
    setFormLoading(true)
    setFormFeedback(null)
    try {
      if (formMode === 'add') {
        await apiProvider.getUserApi().apiUserExchangesExchangePost(ExchangeName[exchange], {
          apiKey: formData.apiKey,
          secretKey: formData.secretKey,
          isTestnet: formData.isTestnet,
        })
      } else {
        await apiProvider.getUserApi().apiUserExchangesExchangePut(ExchangeName[exchange], {
          apiKey: formData.apiKey,
          secretKey: formData.secretKey,
          isTestnet: formData.isTestnet,
        })
      }
      await fetchExchanges()
      closeForm()
    } catch (err) {
      setFormFeedback(extractApiError(err) ?? 'Failed to save exchange connection.')
    } finally {
      setFormLoading(false)
    }
  }

  async function handleDelete(exchange: ExchangeKey) {
    setDeleteLoading(exchange)
    if (activeForm === exchange) closeForm()
    try {
      await apiProvider.getUserApi().apiUserExchangesExchangeDelete(ExchangeName[exchange])
      await fetchExchanges()
    } finally {
      setDeleteLoading(null)
    }
  }

  return (
    <main className="settings-page">
      <div className="page-header">
        <div className="page-header-text">
          <h1>Settings</h1>
          <p>Manage your account and exchange connections</p>
        </div>
      </div>

      <div className="settings-tabs">
        <button
          className={`settings-tab-btn${activeTab === 'profile' ? ' active' : ''}`}
          onClick={() => setActiveTab('profile')}
        >
          Profile
        </button>
        <button
          className={`settings-tab-btn${activeTab === 'apikeys' ? ' active' : ''}`}
          onClick={() => setActiveTab('apikeys')}
        >
          API Keys
        </button>
      </div>

      {activeTab === 'profile' && (
        <ProfileTab
          user={user}
          userLoading={userLoading}
          pwForm={pwForm}
          setPwForm={setPwForm}
          pwLoading={pwLoading}
          pwFeedback={pwFeedback}
          showPw={showPw}
          setShowPw={setShowPw}
          onSubmit={handlePasswordSubmit}
        />
      )}

      {activeTab === 'apikeys' && (
        <ApiKeysTab
          exchanges={exchanges}
          exchangesLoading={exchangesLoading}
          activeForm={activeForm}
          formMode={formMode}
          formData={formData}
          setFormData={setFormData}
          formLoading={formLoading}
          formFeedback={formFeedback}
          showSecret={showSecret}
          setShowSecret={setShowSecret}
          deleteLoading={deleteLoading}
          onOpenAdd={openAddForm}
          onOpenEdit={openEditForm}
          onClose={closeForm}
          onSave={handleSave}
          onDelete={handleDelete}
        />
      )}
    </main>
  )
}
