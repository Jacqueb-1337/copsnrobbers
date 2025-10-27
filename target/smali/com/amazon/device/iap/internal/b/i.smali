.class public abstract Lcom/amazon/device/iap/internal/b/i;
.super Lcom/amazon/android/framework/task/command/AbstractCommandTask;
.source "KiwiCommand.java"


# static fields
.field private static final a:Ljava/lang/String;


# instance fields
.field private final b:Lcom/amazon/device/iap/internal/b/e;

.field private final c:Ljava/lang/String;

.field private final d:Ljava/lang/String;

.field private final e:Ljava/lang/String;

.field private final f:Ljava/util/Map;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Ljava/util/Map",
            "<",
            "Ljava/lang/String;",
            "Ljava/lang/Object;",
            ">;"
        }
    .end annotation
.end field

.field private final g:Lcom/amazon/android/licensing/LicenseFailurePromptContentMapper;

.field private h:Z

.field private i:Lcom/amazon/device/iap/internal/b/i;

.field private j:Lcom/amazon/device/iap/internal/b/i;

.field private k:Z


# direct methods
.method static constructor <clinit>()V
    .locals 1

    .prologue
    .line 24
    const-class v0, Lcom/amazon/device/iap/internal/b/i;

    invoke-virtual {v0}, Ljava/lang/Class;->getSimpleName()Ljava/lang/String;

    move-result-object v0

    sput-object v0, Lcom/amazon/device/iap/internal/b/i;->a:Ljava/lang/String;

    return-void
.end method

.method public constructor <init>(Lcom/amazon/device/iap/internal/b/e;Ljava/lang/String;Ljava/lang/String;)V
    .locals 4

    .prologue
    const/4 v3, 0x0

    .line 43
    invoke-direct {p0}, Lcom/amazon/android/framework/task/command/AbstractCommandTask;-><init>()V

    .line 32
    new-instance v0, Lcom/amazon/android/licensing/LicenseFailurePromptContentMapper;

    invoke-direct {v0}, Lcom/amazon/android/licensing/LicenseFailurePromptContentMapper;-><init>()V

    iput-object v0, p0, Lcom/amazon/device/iap/internal/b/i;->g:Lcom/amazon/android/licensing/LicenseFailurePromptContentMapper;

    .line 38
    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/amazon/device/iap/internal/b/i;->k:Z

    .line 44
    iput-object p1, p0, Lcom/amazon/device/iap/internal/b/i;->b:Lcom/amazon/device/iap/internal/b/e;

    .line 45
    invoke-virtual {p1}, Lcom/amazon/device/iap/internal/b/e;->c()Lcom/amazon/device/iap/model/RequestId;

    move-result-object v0

    invoke-virtual {v0}, Lcom/amazon/device/iap/model/RequestId;->toString()Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/amazon/device/iap/internal/b/i;->c:Ljava/lang/String;

    .line 46
    iput-object p2, p0, Lcom/amazon/device/iap/internal/b/i;->d:Ljava/lang/String;

    .line 47
    iput-object p3, p0, Lcom/amazon/device/iap/internal/b/i;->e:Ljava/lang/String;

    .line 49
    new-instance v0, Ljava/util/HashMap;

    invoke-direct {v0}, Ljava/util/HashMap;-><init>()V

    iput-object v0, p0, Lcom/amazon/device/iap/internal/b/i;->f:Ljava/util/Map;

    .line 50
    iget-object v0, p0, Lcom/amazon/device/iap/internal/b/i;->f:Ljava/util/Map;

    const-string v1, "requestId"

    iget-object v2, p0, Lcom/amazon/device/iap/internal/b/i;->c:Ljava/lang/String;

    invoke-interface {v0, v1, v2}, Ljava/util/Map;->put(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;

    .line 51
    iget-object v0, p0, Lcom/amazon/device/iap/internal/b/i;->f:Ljava/util/Map;

    const-string v1, "sdkVersion"

    const-string v2, "2.0.0"

    invoke-interface {v0, v1, v2}, Ljava/util/Map;->put(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;

    .line 52
    const/4 v0, 0x1

    iput-boolean v0, p0, Lcom/amazon/device/iap/internal/b/i;->h:Z

    .line 54
    iput-object v3, p0, Lcom/amazon/device/iap/internal/b/i;->i:Lcom/amazon/device/iap/internal/b/i;

    .line 55
    iput-object v3, p0, Lcom/amazon/device/iap/internal/b/i;->j:Lcom/amazon/device/iap/internal/b/i;

    .line 56
    return-void
.end method

.method private a(Lcom/amazon/android/framework/prompt/PromptContent;)V
    .locals 2

    .prologue
    .line 112
    if-nez p1, :cond_0

    .line 118
    :goto_0
    return-void

    .line 115
    :cond_0
    new-instance v0, Lcom/amazon/device/iap/internal/b/b;

    invoke-direct {v0, p1}, Lcom/amazon/device/iap/internal/b/b;-><init>(Lcom/amazon/android/framework/prompt/PromptContent;)V

    .line 116
    invoke-static {}, Lcom/amazon/android/Kiwi;->getPromptManager()Lcom/amazon/android/framework/prompt/PromptManager;

    move-result-object v1

    .line 117
    invoke-interface {v1, v0}, Lcom/amazon/android/framework/prompt/PromptManager;->present(Lcom/amazon/android/framework/prompt/Prompt;)V

    goto :goto_0
.end method


# virtual methods
.method public a(Z)Lcom/amazon/device/iap/internal/b/i;
    .locals 0

    .prologue
    .line 59
    iput-boolean p1, p0, Lcom/amazon/device/iap/internal/b/i;->k:Z

    .line 60
    return-object p0
.end method

.method public a(Lcom/amazon/device/iap/internal/b/i;)V
    .locals 0

    .prologue
    .line 68
    iput-object p1, p0, Lcom/amazon/device/iap/internal/b/i;->i:Lcom/amazon/device/iap/internal/b/i;

    .line 69
    return-void
.end method

.method protected a(Ljava/lang/String;Ljava/lang/Object;)V
    .locals 1

    .prologue
    .line 76
    iget-object v0, p0, Lcom/amazon/device/iap/internal/b/i;->f:Ljava/util/Map;

    invoke-interface {v0, p1, p2}, Ljava/util/Map;->put(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;

    .line 77
    return-void
.end method

.method protected abstract a(Lcom/amazon/venezia/command/SuccessResult;)Z
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Ljava/lang/Exception;
        }
    .end annotation
.end method

.method public a_()V
    .locals 0

    .prologue
    .line 202
    invoke-static {p0}, Lcom/amazon/android/Kiwi;->addCommandToCommandTaskPipeline(Lcom/amazon/android/framework/task/command/AbstractCommandTask;)V

    .line 203
    return-void
.end method

.method protected b()Lcom/amazon/device/iap/internal/b/e;
    .locals 1

    .prologue
    .line 80
    iget-object v0, p0, Lcom/amazon/device/iap/internal/b/i;->b:Lcom/amazon/device/iap/internal/b/e;

    return-object v0
.end method

.method public b(Lcom/amazon/device/iap/internal/b/i;)V
    .locals 0

    .prologue
    .line 72
    iput-object p1, p0, Lcom/amazon/device/iap/internal/b/i;->j:Lcom/amazon/device/iap/internal/b/i;

    .line 73
    return-void
.end method

.method protected b(Z)V
    .locals 0

    .prologue
    .line 108
    iput-boolean p1, p0, Lcom/amazon/device/iap/internal/b/i;->h:Z

    .line 109
    return-void
.end method

.method protected c()Ljava/lang/String;
    .locals 1

    .prologue
    .line 84
    iget-object v0, p0, Lcom/amazon/device/iap/internal/b/i;->c:Ljava/lang/String;

    return-object v0
.end method

.method protected getCommandData()Ljava/util/Map;
    .locals 1
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "()",
            "Ljava/util/Map",
            "<",
            "Ljava/lang/String;",
            "Ljava/lang/Object;",
            ">;"
        }
    .end annotation

    .prologue
    .line 104
    iget-object v0, p0, Lcom/amazon/device/iap/internal/b/i;->f:Ljava/util/Map;

    return-object v0
.end method

.method protected getCommandName()Ljava/lang/String;
    .locals 1

    .prologue
    .line 94
    iget-object v0, p0, Lcom/amazon/device/iap/internal/b/i;->d:Ljava/lang/String;

    return-object v0
.end method

.method protected getCommandVersion()Ljava/lang/String;
    .locals 1

    .prologue
    .line 99
    iget-object v0, p0, Lcom/amazon/device/iap/internal/b/i;->e:Ljava/lang/String;

    return-object v0
.end method

.method protected isExecutionNeeded()Z
    .locals 1

    .prologue
    .line 89
    const/4 v0, 0x1

    return v0
.end method

.method protected final onException(Lcom/amazon/android/framework/exception/KiwiException;)V
    .locals 3

    .prologue
    .line 178
    sget-object v0, Lcom/amazon/device/iap/internal/b/i;->a:Ljava/lang/String;

    new-instance v1, Ljava/lang/StringBuilder;

    invoke-direct {v1}, Ljava/lang/StringBuilder;-><init>()V

    const-string v2, "onException: exception = "

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {p1}, Lcom/amazon/android/framework/exception/KiwiException;->getMessage()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-static {v0, v1}, Lcom/amazon/device/iap/internal/util/e;->a(Ljava/lang/String;Ljava/lang/String;)V

    .line 182
    const-string v0, "UNHANDLED_EXCEPTION"

    invoke-virtual {p1}, Lcom/amazon/android/framework/exception/KiwiException;->getType()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_1

    const-string v0, "2.0"

    iget-object v1, p0, Lcom/amazon/device/iap/internal/b/i;->e:Ljava/lang/String;

    invoke-virtual {v0, v1}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_1

    iget-object v0, p0, Lcom/amazon/device/iap/internal/b/i;->j:Lcom/amazon/device/iap/internal/b/i;

    if-eqz v0, :cond_1

    .line 184
    iget-object v0, p0, Lcom/amazon/device/iap/internal/b/i;->j:Lcom/amazon/device/iap/internal/b/i;

    iget-boolean v1, p0, Lcom/amazon/device/iap/internal/b/i;->k:Z

    invoke-virtual {v0, v1}, Lcom/amazon/device/iap/internal/b/i;->a(Z)Lcom/amazon/device/iap/internal/b/i;

    .line 185
    iget-object v0, p0, Lcom/amazon/device/iap/internal/b/i;->j:Lcom/amazon/device/iap/internal/b/i;

    invoke-virtual {v0}, Lcom/amazon/device/iap/internal/b/i;->a_()V

    .line 196
    :cond_0
    :goto_0
    return-void

    .line 188
    :cond_1
    iget-boolean v0, p0, Lcom/amazon/device/iap/internal/b/i;->h:Z

    if-eqz v0, :cond_2

    .line 189
    iget-object v0, p0, Lcom/amazon/device/iap/internal/b/i;->g:Lcom/amazon/android/licensing/LicenseFailurePromptContentMapper;

    invoke-virtual {v0, p1}, Lcom/amazon/android/licensing/LicenseFailurePromptContentMapper;->map(Lcom/amazon/android/framework/exception/KiwiException;)Lcom/amazon/android/framework/prompt/PromptContent;

    move-result-object v0

    .line 190
    invoke-direct {p0, v0}, Lcom/amazon/device/iap/internal/b/i;->a(Lcom/amazon/android/framework/prompt/PromptContent;)V

    .line 192
    :cond_2
    iget-boolean v0, p0, Lcom/amazon/device/iap/internal/b/i;->k:Z

    if-nez v0, :cond_0

    .line 193
    iget-object v0, p0, Lcom/amazon/device/iap/internal/b/i;->b:Lcom/amazon/device/iap/internal/b/e;

    invoke-virtual {v0}, Lcom/amazon/device/iap/internal/b/e;->b()V

    goto :goto_0
.end method

.method protected final onFailure(Lcom/amazon/venezia/command/FailureResult;)V
    .locals 5
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Landroid/os/RemoteException;,
            Lcom/amazon/android/framework/exception/KiwiException;
        }
    .end annotation

    .prologue
    .line 145
    sget-object v0, Lcom/amazon/device/iap/internal/b/i;->a:Ljava/lang/String;

    new-instance v1, Ljava/lang/StringBuilder;

    invoke-direct {v1}, Ljava/lang/StringBuilder;-><init>()V

    const-string v2, "onFailure: result = "

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-static {v0, v1}, Lcom/amazon/device/iap/internal/util/e;->a(Ljava/lang/String;Ljava/lang/String;)V

    .line 147
    const/4 v1, 0x0

    .line 148
    if-eqz p1, :cond_3

    .line 149
    invoke-interface {p1}, Lcom/amazon/venezia/command/FailureResult;->getExtensionData()Ljava/util/Map;

    move-result-object v0

    const-string v2, "maxVersion"

    invoke-interface {v0, v2}, Ljava/util/Map;->get(Ljava/lang/Object;)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Ljava/lang/String;

    .line 150
    if-eqz v0, :cond_3

    const-string v2, "1.0"

    invoke-virtual {v0, v2}, Ljava/lang/String;->equalsIgnoreCase(Ljava/lang/String;)Z

    move-result v0

    if-eqz v0, :cond_3

    .line 151
    const/4 v0, 0x1

    .line 155
    :goto_0
    if-eqz v0, :cond_1

    iget-object v0, p0, Lcom/amazon/device/iap/internal/b/i;->j:Lcom/amazon/device/iap/internal/b/i;

    if-eqz v0, :cond_1

    .line 156
    iget-object v0, p0, Lcom/amazon/device/iap/internal/b/i;->j:Lcom/amazon/device/iap/internal/b/i;

    iget-boolean v1, p0, Lcom/amazon/device/iap/internal/b/i;->k:Z

    invoke-virtual {v0, v1}, Lcom/amazon/device/iap/internal/b/i;->a(Z)Lcom/amazon/device/iap/internal/b/i;

    .line 157
    iget-object v0, p0, Lcom/amazon/device/iap/internal/b/i;->j:Lcom/amazon/device/iap/internal/b/i;

    invoke-virtual {v0}, Lcom/amazon/device/iap/internal/b/i;->a_()V

    .line 171
    :cond_0
    :goto_1
    return-void

    .line 161
    :cond_1
    iget-boolean v0, p0, Lcom/amazon/device/iap/internal/b/i;->h:Z

    if-eqz v0, :cond_2

    .line 162
    new-instance v0, Lcom/amazon/android/framework/prompt/PromptContent;

    invoke-interface {p1}, Lcom/amazon/venezia/command/FailureResult;->getDisplayableName()Ljava/lang/String;

    move-result-object v1

    invoke-interface {p1}, Lcom/amazon/venezia/command/FailureResult;->getDisplayableMessage()Ljava/lang/String;

    move-result-object v2

    invoke-interface {p1}, Lcom/amazon/venezia/command/FailureResult;->getButtonLabel()Ljava/lang/String;

    move-result-object v3

    invoke-interface {p1}, Lcom/amazon/venezia/command/FailureResult;->show()Z

    move-result v4

    invoke-direct {v0, v1, v2, v3, v4}, Lcom/amazon/android/framework/prompt/PromptContent;-><init>(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Z)V

    .line 166
    invoke-direct {p0, v0}, Lcom/amazon/device/iap/internal/b/i;->a(Lcom/amazon/android/framework/prompt/PromptContent;)V

    .line 168
    :cond_2
    iget-boolean v0, p0, Lcom/amazon/device/iap/internal/b/i;->k:Z

    if-nez v0, :cond_0

    .line 169
    iget-object v0, p0, Lcom/amazon/device/iap/internal/b/i;->b:Lcom/amazon/device/iap/internal/b/e;

    invoke-virtual {v0}, Lcom/amazon/device/iap/internal/b/e;->b()V

    goto :goto_1

    :cond_3
    move v0, v1

    goto :goto_0
.end method

.method protected final onSuccess(Lcom/amazon/venezia/command/SuccessResult;)V
    .locals 5
    .annotation system Ldalvik/annotation/Throws;
        value = {
            Landroid/os/RemoteException;
        }
    .end annotation

    .prologue
    .line 122
    sget-object v0, Lcom/amazon/device/iap/internal/b/i;->a:Ljava/lang/String;

    new-instance v1, Ljava/lang/StringBuilder;

    invoke-direct {v1}, Ljava/lang/StringBuilder;-><init>()V

    const-string v2, "onSuccess: result = "

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-static {v0, v1}, Lcom/amazon/device/iap/internal/util/e;->a(Ljava/lang/String;Ljava/lang/String;)V

    .line 123
    const/4 v0, 0x0

    .line 125
    :try_start_0
    invoke-virtual {p0, p1}, Lcom/amazon/device/iap/internal/b/i;->a(Lcom/amazon/venezia/command/SuccessResult;)Z
    :try_end_0
    .catch Ljava/lang/Exception; {:try_start_0 .. :try_end_0} :catch_0

    move-result v0

    .line 130
    :goto_0
    if-eqz v0, :cond_1

    iget-object v1, p0, Lcom/amazon/device/iap/internal/b/i;->i:Lcom/amazon/device/iap/internal/b/i;

    if-eqz v1, :cond_1

    .line 131
    iget-object v0, p0, Lcom/amazon/device/iap/internal/b/i;->i:Lcom/amazon/device/iap/internal/b/i;

    invoke-virtual {v0}, Lcom/amazon/device/iap/internal/b/i;->a_()V

    .line 141
    :cond_0
    :goto_1
    return-void

    .line 126
    :catch_0
    move-exception v1

    .line 127
    sget-object v2, Lcom/amazon/device/iap/internal/b/i;->a:Ljava/lang/String;

    new-instance v3, Ljava/lang/StringBuilder;

    invoke-direct {v3}, Ljava/lang/StringBuilder;-><init>()V

    const-string v4, "Error calling onResult: "

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    invoke-virtual {v3, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-static {v2, v1}, Lcom/amazon/device/iap/internal/util/e;->b(Ljava/lang/String;Ljava/lang/String;)V

    goto :goto_0

    .line 134
    :cond_1
    iget-boolean v1, p0, Lcom/amazon/device/iap/internal/b/i;->k:Z

    if-nez v1, :cond_0

    .line 135
    if-eqz v0, :cond_2

    .line 136
    iget-object v0, p0, Lcom/amazon/device/iap/internal/b/i;->b:Lcom/amazon/device/iap/internal/b/e;

    invoke-virtual {v0}, Lcom/amazon/device/iap/internal/b/e;->a()V

    goto :goto_1

    .line 138
    :cond_2
    iget-object v0, p0, Lcom/amazon/device/iap/internal/b/i;->b:Lcom/amazon/device/iap/internal/b/e;

    invoke-virtual {v0}, Lcom/amazon/device/iap/internal/b/e;->b()V

    goto :goto_1
.end method
