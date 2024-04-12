# Functions(C#) Azure Batch 実行 I/Fサンプル


## 準備
### ファイル リネーム
- `_env` ⇒ `.evn`
- REST CLIENTをインストール</br>
[REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client "REST Client")


### Batch アカウント作成後
`.env` ファイルを環境に合わせて編集
```
TENANT_ID=<TENANT_ID>
SUBSCRIPTOIN_GUID=<SUBSCRIPTOIN_GUID>
RESOURCE_GROUP=<RESOURCE_GROUP>
LOCATION=japaneast
BATCH_POOL_ID=DotNetQuickstartPool
BATCH_JOB_ID=DotNetQuickstartJob
BATCH_ACCOUNT_URL=<BATCH_ACCOUNT_URL>
BATCH_ACCOUNT_NAME=<BATCH_ACCOUNT_NAME>
BATCH_ACCOUNT_KEY=<BATCH_ACCOUNT_KEY>
```
## 実行
1. 
1. クォータの解除
1. `azure-batch-multiple-task.ipynb` `azure-batch-sample.ipynb`で作成したBatchアカウント、プールは削除せずに実行する