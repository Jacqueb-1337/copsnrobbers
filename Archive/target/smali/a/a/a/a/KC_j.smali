.class final La/a/a/a/KC_j;
.super Ljava/lang/Object;
.source "SourceFile"

# interfaces
.implements Landroid/os/Parcelable;


# static fields
.field public static final CREATOR:Landroid/os/Parcelable$Creator;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Landroid/os/Parcelable$Creator",
            "<",
            "La/a/a/a/KC_j;",
            ">;"
        }
    .end annotation
.end field


# instance fields
.field a:[La/a/a/a/KC_l;

.field b:[I

.field c:[La/a/a/a/KC_c;


# direct methods
.method static constructor <clinit>()V
    .locals 1

    .prologue
    .line 383
    new-instance v0, La/a/a/a/KC_j$1;

    invoke-direct {v0}, La/a/a/a/KC_j$1;-><init>()V

    sput-object v0, La/a/a/a/KC_j;->CREATOR:Landroid/os/Parcelable$Creator;

    return-void
.end method

.method public constructor <init>()V
    .locals 0

    .prologue
    .line 364
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 365
    return-void
.end method

.method public constructor <init>(Landroid/os/Parcel;)V
    .locals 1

    .prologue
    .line 367
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 368
    sget-object v0, La/a/a/a/KC_l;->CREATOR:Landroid/os/Parcelable$Creator;

    invoke-virtual {p1, v0}, Landroid/os/Parcel;->createTypedArray(Landroid/os/Parcelable$Creator;)[Ljava/lang/Object;

    move-result-object v0

    check-cast v0, [La/a/a/a/KC_l;

    iput-object v0, p0, La/a/a/a/KC_j;->a:[La/a/a/a/KC_l;

    .line 369
    invoke-virtual {p1}, Landroid/os/Parcel;->createIntArray()[I

    move-result-object v0

    iput-object v0, p0, La/a/a/a/KC_j;->b:[I

    .line 370
    sget-object v0, La/a/a/a/KC_c;->CREATOR:Landroid/os/Parcelable$Creator;

    invoke-virtual {p1, v0}, Landroid/os/Parcel;->createTypedArray(Landroid/os/Parcelable$Creator;)[Ljava/lang/Object;

    move-result-object v0

    check-cast v0, [La/a/a/a/KC_c;

    iput-object v0, p0, La/a/a/a/KC_j;->c:[La/a/a/a/KC_c;

    .line 371
    return-void
.end method


# virtual methods
.method public final describeContents()I
    .locals 1

    .prologue
    .line 374
    const/4 v0, 0x0

    return v0
.end method

.method public final writeToParcel(Landroid/os/Parcel;I)V
    .locals 1
    .param p1, "dest"    # Landroid/os/Parcel;
    .param p2, "flags"    # I

    .prologue
    .line 378
    iget-object v0, p0, La/a/a/a/KC_j;->a:[La/a/a/a/KC_l;

    invoke-virtual {p1, v0, p2}, Landroid/os/Parcel;->writeTypedArray([Landroid/os/Parcelable;I)V

    .line 379
    iget-object v0, p0, La/a/a/a/KC_j;->b:[I

    invoke-virtual {p1, v0}, Landroid/os/Parcel;->writeIntArray([I)V

    .line 380
    iget-object v0, p0, La/a/a/a/KC_j;->c:[La/a/a/a/KC_c;

    invoke-virtual {p1, v0, p2}, Landroid/os/Parcel;->writeTypedArray([Landroid/os/Parcelable;I)V

    .line 381
    return-void
.end method
