.class public final Lcom/kamcord/android/core/KC_c;
.super Ljava/lang/Object;


# instance fields
.field public a:Ljava/lang/String;

.field public b:Ljava/lang/String;

.field public c:I

.field public d:Lcom/kamcord/android/core/KC_f;


# direct methods
.method public constructor <init>(Lcom/kamcord/android/core/KC_c;)V
    .locals 1

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    iget-object v0, p1, Lcom/kamcord/android/core/KC_c;->a:Ljava/lang/String;

    iput-object v0, p0, Lcom/kamcord/android/core/KC_c;->a:Ljava/lang/String;

    iget-object v0, p1, Lcom/kamcord/android/core/KC_c;->b:Ljava/lang/String;

    iput-object v0, p0, Lcom/kamcord/android/core/KC_c;->b:Ljava/lang/String;

    iget v0, p1, Lcom/kamcord/android/core/KC_c;->c:I

    iput v0, p0, Lcom/kamcord/android/core/KC_c;->c:I

    iget-object v0, p1, Lcom/kamcord/android/core/KC_c;->d:Lcom/kamcord/android/core/KC_f;

    iput-object v0, p0, Lcom/kamcord/android/core/KC_c;->d:Lcom/kamcord/android/core/KC_f;

    return-void
.end method

.method public constructor <init>(Ljava/lang/String;Ljava/lang/String;ILcom/kamcord/android/core/KC_f;)V
    .locals 1

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    iput-object p1, p0, Lcom/kamcord/android/core/KC_c;->a:Ljava/lang/String;

    iput-object p2, p0, Lcom/kamcord/android/core/KC_c;->b:Ljava/lang/String;

    iput p3, p0, Lcom/kamcord/android/core/KC_c;->c:I

    const/4 v0, 0x0

    iput-object v0, p0, Lcom/kamcord/android/core/KC_c;->d:Lcom/kamcord/android/core/KC_f;

    return-void
.end method


# virtual methods
.method public final a(Ljava/lang/String;[I)Z
    .locals 6

    const/4 v0, 0x1

    const/4 v1, 0x0

    iget-object v2, p0, Lcom/kamcord/android/core/KC_c;->b:Ljava/lang/String;

    invoke-virtual {p1, v2}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v2

    if-eqz v2, :cond_2

    iget v3, p0, Lcom/kamcord/android/core/KC_c;->c:I

    array-length v4, p2

    move v2, v1

    :goto_0
    if-ge v2, v4, :cond_1

    aget v5, p2, v2

    if-ne v3, v5, :cond_0

    move v2, v0

    :goto_1
    if-eqz v2, :cond_2

    :goto_2
    return v0

    :cond_0
    add-int/lit8 v2, v2, 0x1

    goto :goto_0

    :cond_1
    move v2, v1

    goto :goto_1

    :cond_2
    move v0, v1

    goto :goto_2
.end method
